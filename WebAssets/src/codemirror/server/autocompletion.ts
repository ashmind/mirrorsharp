import { startCompletion, acceptCompletion, closeCompletion, completionStatus, autocompletion, CompletionSource, Completion } from '@codemirror/autocomplete';
import { Prec } from '@codemirror/state';
import { ViewPlugin, EditorView, keymap } from '@codemirror/view';
import { renderParts } from '../../helpers/render-parts';
import type { Connection } from '../../protocol/connection';
import type { CompletionInfoMessage, CompletionsMessage } from '../../protocol/messages';
import { applyChangesFromServer } from '../helpers/apply-changes-from-server';

export const autocompletionFromServer = <O, U>(connection: Connection<O, U>) => {
    // Since completions are scoped per connection (server will have one active completion list per connection),
    // it's OK to have state per plugin rather than per view
    let currentCompletionsMessage = null as CompletionsMessage | null;
    const resolveInfoList = new Array<((info: Node) => void) | undefined>();

    const applyCompletion = (index: number) => {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        connection.sendCompletionState(index);
    };

    const requestCompletionInfo = (index: number, ref: { info?: Promise<Node> }) => {
        if (!ref.info) {
            ref.info = new Promise(resolve => { resolveInfoList[index] = resolve; });
            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            connection.sendCompletionState('info', index);
        }
        return ref.info;
    };

    const receiveCompletionInfo = (view: EditorView, message: CompletionInfoMessage) => {
        const resolve = resolveInfoList[message.index];
        if (!resolve)
            return;
        resolve(renderParts(message.parts, { splitLinesToSections: true }));
    };

    const getAndFilterCompletions = (context => {
        const all = currentCompletionsMessage
            ?.completions
            .map((data, index) => ({ data, index })) ?? [];
        const prefix = context.matchBefore(/[\w\d]+/);

        let filtered = all;
        if (prefix) {
            const prefixTextLowerCase = prefix.text.toLowerCase();
            filtered = all.filter(
                ({ data }) => (data.filterText ?? data.displayText).toLowerCase().startsWith(prefixTextLowerCase)
            );
        }

        const completions = filtered.map(({ data, index }) => {
            const infoRef = {};
            return ({
                label: data.displayText,
                type: data.kinds[0],
                apply: () => applyCompletion(index),
                info: () => requestCompletionInfo(index, infoRef)
            } as Completion);
        });

        return {
            from: prefix?.from ?? context.pos,
            options: completions
        };
    }) satisfies CompletionSource;

    const receiveCompletionMessagesFromServer = ViewPlugin.define(view => {
        const removeListeners = connection.addEventListeners({
            message: message => {
                if (message.type === 'completions') {
                    currentCompletionsMessage = message;
                    startCompletion(view);
                    return;
                }

                if (message.type === 'changes' && message.reason === 'completion') {
                    applyChangesFromServer(view, message.changes);
                    closeCompletion(view);
                    currentCompletionsMessage = null;
                }

                if (message.type === 'completionInfo')
                    receiveCompletionInfo(view, message);
            }
        });

        return {
            destroy: removeListeners
        };
    });

    const sendCancelCompletionToServer = ViewPlugin.define(() => ({
        update: u => {
            const previousStatus = completionStatus(u.startState);
            if (previousStatus === null)
                return;

            const currentStatus = completionStatus(u.state);
            if (currentStatus !== null)
                return;

            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            connection.sendCompletionState('cancel');
            currentCompletionsMessage = null;
        }
    }));

    const acceptCompletionOnCommitChar = EditorView.domEventHandlers({
        keydown({ key }, view) {
            if (key.length > 1) // control keys
                return;

            if (!currentCompletionsMessage?.commitChars.includes(key))
                return;

            acceptCompletion(view);
        }
    });

    const forceCompletionOnCtrlSpace = Prec.highest(keymap.of([{ key: 'Ctrl-Space', run: () => {
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
        // overrides default autocompletion binding
        forceCompletionOnCtrlSpace,
        autocompletion({
            activateOnTyping: false,
            override: [getAndFilterCompletions]
        }),
        //closeCompletionsWhenFullyFilteredOut,
        receiveCompletionMessagesFromServer,
        sendCancelCompletionToServer,
        acceptCompletionOnCommitChar,
        acceptCompletionOnTab
    ];
};