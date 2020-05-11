import type { Connection } from '../../connection';
import { EditorState } from '@codemirror/next/state';
import lineSeparator from '../line-separator';

export const sendChangesToServer = <O, U>(connection: Connection<O, U>) => EditorState.changeFilter.of(({ from, to, text }, state) => {
    const cursorOffset = state.selection.primary.from;
    if (from === cursorOffset && to === from && text.length === 1 && text[0].length === 1) {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        connection.sendTypeChar(text[0][0]);
        return null;
    }

    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    connection.sendReplaceText(from, to - from, text.join(lineSeparator), 0);
    return null;
});