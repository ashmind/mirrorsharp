import type { EditorView } from '@codemirror/view';

export const getText = (view: EditorView) => {
    return view.state.sliceDoc();
};