import { EditorState, EditorSelection } from '@codemirror/next/state';
import { history } from '@codemirror/next/history';
import { keymap } from '@codemirror/next/keymap';
import { baseKeymap } from '@codemirror/next/commands';
import { defaultHighlighter } from '@codemirror/next/highlight';
import type { Connection } from '../connection';
import type { SlowUpdateOptions } from '../../interfaces/slow-update';
import { csharp } from './lang-csharp';
import { sendChangesToServer } from './server/send-changes';
import { sendCursorToServer } from './server/send-cursor';
import { slowUpdateLinter } from './server/slow-update-linter';
import lineSeparator from './line-separator';

export function createState<O, U>(
    connection: Connection<O, U>,
    options: {
        initialText?: string;
        initialCursorOffset?: number;
    } & SlowUpdateOptions<U>
) {
    return EditorState.create({
        ...(options.initialText ? { doc: options.initialText } : {}),
        ...(options.initialCursorOffset ? { selection: EditorSelection.single(options.initialCursorOffset) } : {}),
        extensions: [
            EditorState.indentUnit.of(4),
            EditorState.lineSeparator.of(lineSeparator),

            history(),
            keymap(baseKeymap),
            csharp(),
            defaultHighlighter,

            sendChangesToServer(connection),
            sendCursorToServer(connection),
            slowUpdateLinter(connection, options)
        ]
    });
}