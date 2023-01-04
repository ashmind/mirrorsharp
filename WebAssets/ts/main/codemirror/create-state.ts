import { EditorState, EditorSelection } from '@codemirror/state';
import { indentUnit, syntaxHighlighting } from '@codemirror/language';
import { history } from '@codemirror/commands';
import type { Connection } from '../connection';
import type { SlowUpdateOptions } from '../../interfaces/slow-update';
import { csharp } from './lang-csharp';
import lineSeparator from './line-separator';
import keymap from './keymap';
import { sendChangesToServer } from './server/send-changes';
import { slowUpdateLinter } from './server/slow-update-linter';
import { connectionState } from './server/connection-state';
import { infotipsFromServer } from './server/infotips';
import { autocompletionFromServer } from './server/autocompletion';
import { classHighlighter } from '@lezer/highlight';
import type { Session } from '../session';

export function createState<O, U>(
    connection: Connection<O, U>,
    session: Session<O>,
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
            syntaxHighlighting(classHighlighter),

            connectionState(connection),
            sendChangesToServer(session as Session),
            slowUpdateLinter(connection, options),
            infotipsFromServer(connection),
            autocompletionFromServer(connection),

            // has to go last so that more specific keymaps
            // in e.g. autocomplete have more priority
            keymap
        ]
    });
}