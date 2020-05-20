import type { Connection } from '../../connection';
import { EditorState } from '@codemirror/next/state';

export const sendCursorToServer = <O, U>(connection: Connection<O, U>) => EditorState.changeFilter.of(({ selection }, state) => {
    if (!selection)
        return true;

    if (selection.primary.from !== state.selection.primary.from) {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        connection.sendMoveCursor(selection.primary.from);
    }
    return true;
});