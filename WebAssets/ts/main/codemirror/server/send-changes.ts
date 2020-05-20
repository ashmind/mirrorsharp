import type { Connection } from '../../connection';
import { EditorState, Text } from '@codemirror/next/state';

function sendReplace<O, U>(connection: Connection<O, U>, from: number, to: number, text: Text) {
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    connection.sendReplaceText(from, to - from, text.toString(), 0);
}

export const sendChangesToServer = <O, U>(connection: Connection<O, U>) => EditorState.changeFilter.of(({ changes }, state) => {
    const cursorOffset = state.selection.primary.from;

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
            sendReplace(connection, firstFrom!, firstTo!, firstText!);
            return true;
        }

        sendReplace(connection, from, to - from, inserted);
        return true;
    });

    if (changeCount === 1) {
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        if (firstFrom === cursorOffset && firstTo === firstFrom && firstText!.length === 1) {
            // eslint-disable-next-line @typescript-eslint/no-floating-promises, @typescript-eslint/no-non-null-assertion
            connection.sendTypeChar(firstText!.line(1).slice(0, 1));
            return true;
        }

        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        sendReplace(connection, firstFrom!, firstTo!, firstText!);
    }

    return true;
});