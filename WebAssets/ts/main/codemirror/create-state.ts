import { EditorState } from '@codemirror/next/state';
import { history } from '@codemirror/next/history';

export function createState({ initialText }: { initialText?: string }) {
    return EditorState.create({
        doc: initialText,
        extensions: [
            history()
        ]
    });
}