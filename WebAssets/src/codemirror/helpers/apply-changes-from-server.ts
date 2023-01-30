import type { ChangeSpec, TransactionSpec } from '@codemirror/state';
import type { EditorView } from '@codemirror/view';
import type { ChangeData } from '../../protocol/messages';
import { convertFromServerPosition } from './convert-position';

export const applyChangesFromServer = (view: EditorView, changesFromServer: ReadonlyArray<ChangeData>) => {
    const [selection] = view.state.selection.ranges;
    const transaction = { changes: [] } as Omit<TransactionSpec, 'changes'> & {
        changes: Array<ChangeSpec>;
    };
    for (const { start, length, text } of changesFromServer) {
        const change = {
            from: convertFromServerPosition(view.state.doc, start),
            to: convertFromServerPosition(view.state.doc, start + length),
            insert: text
        };
        transaction.changes.push(change);
        if (selection && selection.from >= change.from && selection.from <= change.to)
            transaction.selection = { anchor: change.from + change.insert.length };
    }

    view.dispatch(transaction);
};