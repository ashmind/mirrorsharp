import { EditorState, EditorSelection } from '@codemirror/next/state';
import { indentUnit } from '@codemirror/next/language';
import { highlightStyle } from '@codemirror/next/highlight';
import { history } from '@codemirror/next/history';
import type { Connection } from '../connection';
import type { SlowUpdateOptions } from '../../interfaces/slow-update';
import { csharp } from './lang-csharp';
import highlighterSpecs from './highlighter-specs';
import lineSeparator from './line-separator';
import keymap from './keymap';
import { sendChangesToServer } from './server/send-changes';
import { slowUpdateLinter } from './server/slow-update-linter';
import { connectionState } from './server/connection-state';
import { autocompleteFromServer } from './server/autocomplete';

export function createState<O, U>(
    connection: Connection<O, U>,
    options: {
        initialText?: string;
        initialCursorOffset?: number;
    } & SlowUpdateOptions<U> = {}
) {
    return EditorState.create({
        ...(options.initialText ? { doc: options.initialText } : {}),
        ...(options.initialCursorOffset ? { selection: EditorSelection.single(options.initialCursorOffset) } : {}),
        extensions: [
            indentUnit.of('    '),
            EditorState.lineSeparator.of(lineSeparator),

            history(),
            csharp(),
            highlightStyle(...highlighterSpecs),

            connectionState(connection),
            sendChangesToServer(connection),
            slowUpdateLinter(connection, options),
            autocompleteFromServer(connection),

            // has to go last so that more specific keymaps
            // in e.g. autocomplete have more priority
            keymap
        ]
    });
}