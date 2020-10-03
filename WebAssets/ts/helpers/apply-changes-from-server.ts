import type { EditorView } from '@codemirror/next/view';
import type { ChangeData } from '../interfaces/protocol';

export function applyChangesFromServer(view: EditorView, changes: ReadonlyArray<ChangeData>) {
    view.dispatch({ changes: changes.map(c => ({
        from: c.start,
        to: c.start + c.length,
        insert: c.text
    })) });
}