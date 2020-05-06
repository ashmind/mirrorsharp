import type { Connection } from '../../connection';
import { EditorState } from '@codemirror/next/state';
import lineSeparator from '../line-separator';

export const sendChangesToServer = <O, U>(connection: Connection<O, U>) => EditorState.changeFilter.of(change => {
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    connection.sendReplaceText(change.from, change.to - change.from, change.text.join(lineSeparator), 0);
    return null;
});