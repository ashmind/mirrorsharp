import type { Connection } from '../../connection';
import type { CompletionItemData } from '../../../interfaces/protocol';
import { ViewPlugin, EditorView } from '@codemirror/next/view';
import { startCompletion, closeCompletion, completionStatus, autocompletion, CompletionSource, Completion } from '@codemirror/next/autocomplete';
import { addEvents } from '../../../helpers/add-events';
import { defineEffectField } from '../../../helpers/define-effect-field';
import { applyChangesFromServer } from '../../../helpers/apply-changes-from-server';

const [lastCompletionsFromServer, dispatchLastCompletionsFromServerChanged] = defineEffectField<ReadonlyArray<Completion>>([]);
const completionIndexKey = Symbol('completionIndex');

type CompletionWithIndex = Omit<Completion, 'apply'> & {
    [completionIndexKey]: number;
    apply: (view: EditorView, completion: CompletionWithIndex) => void;
};

const getLastServerCompletions = (context => ({
    from: context.pos,
    options: context.state.field(lastCompletionsFromServer)
})) as CompletionSource;

export const autocompleteFromServer = <O, U>(connection: Connection<O, U>) => [
    lastCompletionsFromServer,

    autocompletion({
        activateOnTyping: false,
        override: [getLastServerCompletions]
    }),

    ViewPlugin.define(view => {
        const applyCompletion = ((_, c: CompletionWithIndex) => {
            console.log('before sendCompletionState');
            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            connection.sendCompletionState(c[completionIndexKey]);
        }) as CompletionWithIndex['apply'];

        const mapCompletionFromServer = ({ displayText, kinds }: CompletionItemData, index: number) => ({
            label: displayText,
            type: kinds[0],
            apply: applyCompletion,
            [completionIndexKey]: index
        } as CompletionWithIndex);

        const removeEvents = addEvents(connection, {
            message: message => {
                if (message.type === 'completions') {
                    dispatchLastCompletionsFromServerChanged(view, message.completions.map(mapCompletionFromServer));
                    console.log(message.completions.map(mapCompletionFromServer));
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
            update: u => {
                const previousStatus = completionStatus(u.prevState);
                if (previousStatus === null)
                    return;

                const currentStatus = completionStatus(u.state);
                if (currentStatus !== null)
                    return;

                // eslint-disable-next-line @typescript-eslint/no-floating-promises
                connection.sendCompletionState('cancel');
            },

            destroy: removeEvents
        };
    })
];