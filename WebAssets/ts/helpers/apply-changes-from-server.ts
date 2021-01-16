import type { EditorView } from '@codemirror/next/view';
import type { ChangeData } from '../interfaces/protocol';

export function applyChangesFromServer(view: EditorView, changesFromServer: ReadonlyArray<ChangeData>) {
    const changes = changesFromServer.map(({ start, length, text }) => ({
        from: start,
        to: start + length,
        insert: text
    }));

    view.dispatch({ changes });
}