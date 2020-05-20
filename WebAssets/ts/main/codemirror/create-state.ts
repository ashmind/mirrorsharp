import { EditorState, EditorSelection } from '@codemirror/next/state';
import { highlighter } from '@codemirror/next/highlight';
import { history } from '@codemirror/next/history';
import { keymap } from '@codemirror/next/keymap';
import type { Connection } from '../connection';
import type { SlowUpdateOptions } from '../../interfaces/slow-update';
import { csharp } from './lang-csharp';
import highlighterSpec from './highlighter-spec';
import lineSeparator from './line-separator';
import keymapSpec from './keymap-spec';
import { sendChangesToServer } from './server/send-changes';
import { sendCursorToServer } from './server/send-cursor';
import { slowUpdateLinter } from './server/slow-update-linter';

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
            keymap(keymapSpec),
            csharp(),
            highlighter(highlighterSpec),

            sendChangesToServer(connection),
            sendCursorToServer(connection),
            slowUpdateLinter(connection, options)
        ]
    });
}