import type { Connection } from '../../connection';
import type { CompletionItemData } from '../../../interfaces/protocol';
import { ViewPlugin, EditorView } from '@codemirror/next/view';
import { startCompletion, closeCompletion, completionStatus, autocompletion, CompletionSource, Completion } from '@codemirror/next/autocomplete';
import { addEvents } from '../../../helpers/add-events';
import { defineEffectField } from '../../../helpers/define-effect-field';
import { applyChangesFromServer } from '../../../helpers/apply-changes-from-server';

const [lastCompletionsFromServer, dispatchLastCompletionsFromServerChanged] = defineEffectField<ReadonlyArray<CompletionItemData>>([]);
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
        const all = context.state.field(lastCompletionsFromServer).map((completion, index) => ({ completion, index }));
        const prefix = context.matchBefore(/[\w\d]+/);

        const filtered = prefix
            ? all.filter(({ completion: c }) => (c.filterText ?? c.displayText).startsWith(prefix.text))
            : all;

        return {
            from: prefix?.from ?? context.pos,
            options: filtered.map(mapCompletionFromServer)
        };
    }) as CompletionSource;

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

    const receiveCompletionMessagesFromServer = ViewPlugin.define(view => {
        const removeEvents = addEvents(connection, {
            message: message => {
                if (message.type === 'completions') {
                    dispatchLastCompletionsFromServerChanged(view, message.completions);
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

    return [
        lastCompletionsFromServer,
        autocompletion({
            activateOnTyping: true,
            override: [getAndFilterCompletions]
        }),
        sendCancelCompletionToServer,
        receiveCompletionMessagesFromServer
    ];
};