import type { Connection } from '../../connection';
import type { CompletionItemData, CompletionsMessage } from '../../../interfaces/protocol';
import { ViewPlugin, EditorView } from '@codemirror/next/view';
import { startCompletion, acceptCompletion, closeCompletion, completionStatus, autocompletion, CompletionSource, Completion } from '@codemirror/next/autocomplete';
import { addEvents } from '../../../helpers/add-events';
import { defineEffectField } from '../../../helpers/define-effect-field';
import { applyChangesFromServer } from '../../../helpers/apply-changes-from-server';

const [lastCompletionsFromServer, dispatchLastCompletionsFromServerChanged] = defineEffectField<CompletionsMessage|null>(null);
const completionIndexKey = Symbol('completionIndex');

type CompletionWithIndex = Omit<Completion, 'apply'> & {
    [completionIndexKey]: number;
    apply: (view: EditorView, completion: CompletionWithIndex) => void;
};

export const autocompleteFromServer = <O, U>(connection: Connection<O, U>) => {
    const applyCompletion = ((_, c: CompletionWithIndex) => {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        connection.sendCompletionState(c[completionIndexKey]);
    }) as CompletionWithIndex['apply'];

    const mapCompletionFromServer = ({ completion: { displayText, kinds }, index }: { completion: CompletionItemData; index: number }) => ({
        label: displayText,
        type: kinds[0],
        apply: applyCompletion,
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
                }
            }
        });

        return {
            destroy: removeEvents
        };
    });

    const sendCancelCompletionToServer = ViewPlugin.define(() => ({
        update: u => {
            const previousStatus = completionStatus(u.prevState);
            if (previousStatus === null)
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

    return [
        lastCompletionsFromServer,
        autocompletion({
            activateOnTyping: true,
            override: [getAndFilterCompletions]
        }),
        receiveCompletionMessagesFromServer,
        sendCancelCompletionToServer,
        acceptCompletionOnCommitChar
    ];
};