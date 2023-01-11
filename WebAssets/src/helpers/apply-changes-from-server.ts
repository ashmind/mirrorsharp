import type { ChangeSpec, TransactionSpec } from '@codemirror/state';
import type { EditorView } from '@codemirror/view';
import type { ChangeData } from '../protocol/messages';

export function applyChangesFromServer(view: EditorView, changesFromServer: ReadonlyArray<ChangeData>) {
    const [selection] = view.state.selection.ranges;
    const transaction = { changes: [] } as Omit<TransactionSpec, 'changes'> & {
        changes: Array<ChangeSpec>;
    };
    for (const { start, length, text } of changesFromServer) {
        const change = {
            from: start,
            to: start + length,
            insert: text
        };
        transaction.changes.push(change);
        if (selection.from >= change.from && selection.from <= change.to)
            transaction.selection = { anchor: change.from + change.insert.length };
    }

    view.dispatch(transaction);
}