import { Transaction } from '@codemirror/state';
import type { EditorView } from '@codemirror/view';

export class TestText {
    readonly #cmView: EditorView;

    constructor(cmView: EditorView) {
        this.#cmView = cmView;
    }

    type(text: string) {
        let cursorOffset = this.#cmView.state.selection.main.anchor;
        for (const char of text) {
            const newCursorOffset = cursorOffset + 1;
            this.#cmView.dispatch(this.#cmView.state.update({
                annotations: [Transaction.userEvent.of('input.type')],
                changes: { from: cursorOffset, insert: char },
                selection: { anchor: newCursorOffset }
            }));
            cursorOffset = newCursorOffset;
        }
    }
}