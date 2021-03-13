import type { Connection } from '../../connection';
import type { CompletionItemData, CompletionsMessage, CompletionInfoMessage } from '../../../interfaces/protocol';
import { Prec } from '@codemirror/state';
import { ViewPlugin, EditorView, keymap } from '@codemirror/view';
import { startCompletion, acceptCompletion, closeCompletion, currentCompletions, completionStatus, autocompletion, CompletionSource, Completion } from '@codemirror/autocomplete';
import { addEvents } from '../../../helpers/add-events';
import { defineEffectField } from '../../../helpers/define-effect-field';
import { applyChangesFromServer } from '../../../helpers/apply-changes-from-server';
import { renderParts } from '../../../helpers/render-parts';

const [lastCompletionsFromServer, dispatchLastCompletionsFromServerChanged] = defineEffectField<CompletionsMessage|null>(null);
const completionIndexKey = Symbol('completionIndex');
const completionInfoNodeKey = Symbol('completionInfoNode');

type CompletionWithIndex = Omit<Completion, 'apply'> & {
    [completionIndexKey]: number;
    // A bit hacky -- might be better to update this in the state
    [completionInfoNodeKey]?: {
        resolve: (node: Node) => void;
        promise: Promise<Node>;
    };
    apply: (view: EditorView, completion: CompletionWithIndex) => void;
    info: (completion: CompletionWithIndex) => Promise<Node>;
};

export const autocompleteFromServer = <O, U>(connection: Connection<O, U>) => {
    const applyCompletion = ((_, c: CompletionWithIndex) => {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        connection.sendCompletionState(c[completionIndexKey]);
    }) as CompletionWithIndex['apply'];

    const requestCompletionInfo = (completion: CompletionWithIndex) => {
        let info = completion[completionInfoNodeKey];
        if (!info) {
            let resolve: ((node: Node) => void)|null = null;
            const promise = new Promise<Node>(r => resolve = r);
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            info = { promise, resolve: resolve! };
            completion[completionInfoNodeKey] = info;
            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            connection.sendCompletionState('info', completion[completionIndexKey]);
        }
        return info.promise;
    };

    const receiveCompletionInfo = (view: EditorView, message: CompletionInfoMessage) => {
        const completion = currentCompletions(view.state)[message.index] as CompletionWithIndex;
        const { resolve } = completion[completionInfoNodeKey] ?? {};
        if (!resolve)
            return;
        resolve(renderParts(message.parts));
    };

    const mapCompletionFromServer = ({ completion: { displayText, kinds }, index }: { completion: CompletionItemData; index: number }) => ({
        label: displayText,
        type: kinds[0],
        apply: applyCompletion,
        info: requestCompletionInfo,
        [completionIndexKey]: index
    } as CompletionWithIndex);

    const getAndFilterCompletions = (context => {
        const all = (context.state.field(lastCompletionsFromServer)
            ?.completions
            .map((completion, index) => ({ completion, index })) ?? []);
        const prefix = context.matchBefore(/[\w\d]+/);

        let filtered = all;
        if (prefix) {
            const prefixTextLowerCase = prefix.text.toLowerCase();
            filtered = all.filter(
                ({ completion: c }) => (c.filterText ?? c.displayText).toLowerCase().startsWith(prefixTextLowerCase)
            );
        }

        return {
            from: prefix?.from ?? context.pos,
            options: filtered.map(mapCompletionFromServer)
        };
    }) as CompletionSource;

    const receiveCompletionMessagesFromServer = ViewPlugin.define(view => {
        const removeEvents = addEvents(connection, {
            message: message => {
                if (message.type === 'completions') {
                    dispatchLastCompletionsFromServerChanged(view, message);
                    startCompletion(view);
                    return;
                }

                if (message.type === 'changes' && message.reason === 'completion') {
                    applyChangesFromServer(view, message.changes);
                    closeCompletion(view);
                    dispatchLastCompletionsFromServerChanged(view, null);
                }

                if (message.type === 'completionInfo')
                    receiveCompletionInfo(view, message);
            }
        });

        return {
            destroy: removeEvents
        };
    });

    const sendCancelCompletionToServer = ViewPlugin.define(() => ({
        update: u => {
            const previousStatus = completionStatus(u.startState);
            if (previousStatus !== 'active')
                return;

            const currentStatus = completionStatus(u.state);
            if (currentStatus !== null)
                return;

            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            connection.sendCompletionState('cancel');
        }
    }));

    const acceptCompletionOnCommitChar = EditorView.domEventHandlers({
        keydown({ key }, view) {
            if (key.length > 1) // control keys
                return;

            const state = view.state.field(lastCompletionsFromServer);
            if (!state?.commitChars.includes(key))
                return;

            acceptCompletion(view);
        }
    });

    const forceCompletionOnCtrlSpace = Prec.override(keymap.of([{ key: 'Ctrl-Space', run: () => {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        connection.sendCompletionState('force');
        return true;
    } }]));

    const acceptCompletionOnTab = keymap.of([{ key: 'Tab', run: view => {
        if (completionStatus(view.state) !== 'active')
            return false;

        acceptCompletion(view);
        return true;
    } }]);

    return [
        lastCompletionsFromServer,
        // overrides default autocompletion binding
        forceCompletionOnCtrlSpace,
        autocompletion({
            activateOnTyping: true,
            override: [getAndFilterCompletions]
        }),
        receiveCompletionMessagesFromServer,
        sendCancelCompletionToServer,
        acceptCompletionOnCommitChar,
        acceptCompletionOnTab
    ];
};