import { EditorState } from '@codemirror/next/state';
import { history } from '@codemirror/next/history';
import { keymap } from '@codemirror/next/keymap';
import { baseKeymap } from '@codemirror/next/commands';

export function createState({ initialText }: { initialText?: string }) {
    return EditorState.create({
        doc: initialText,
        extensions: [
            history(),
            keymap(baseKeymap)
        ]
    });
}