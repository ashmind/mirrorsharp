import type { Connection } from '../../connection';
import { Text, StateField } from '@codemirror/next/state';

export const sendChangesToServer = <O, U>(connection: Connection<O, U>) => {
    function sendReplace<O, U>(from: number, to: number, text: Text) {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        connection.sendReplaceText(from, to - from, text.toString(), 0);
    }

    const lastChangesSent = StateField.define<boolean>({
        create(state) {
            if (state.doc.length === 0)
                return true;

            sendReplace(0, 0, state.doc);
            return true;
        },

        update(_, { changes }, newState) {
            const cursorOffset = newState.selection.primary.from;

            let changeCount = 0;
            let firstFrom: number|undefined;
            let firstTo: number|undefined;
            let firstText: Text|undefined;
            changes.iterChanges((from, to, _f, _t, inserted) => {
                changeCount += 1;
                if (changeCount === 1) {
                    firstFrom = from;
                    firstTo = to;
                    firstText = inserted;
                    return;
                }

                if (changeCount === 2) {
                    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                    sendReplace(firstFrom!, firstTo!, firstText!);
                }

                sendReplace(from, to, inserted);
            });

            if (changeCount === 1) {
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                if (firstFrom === cursorOffset && firstTo === firstFrom && firstText!.length === 1) {
                    // eslint-disable-next-line @typescript-eslint/no-floating-promises, @typescript-eslint/no-non-null-assertion
                    connection.sendTypeChar(firstText!.line(1).slice(0, 1));
                    return true;
                }

                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                sendReplace(firstFrom!, firstTo!, firstText!);
            }

            return true;
        }
    });

    return [lastChangesSent];
};