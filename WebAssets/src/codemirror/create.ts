import { history } from '@codemirror/commands';
import { indentUnit, syntaxHighlighting } from '@codemirror/language';
import { EditorState, EditorSelection, Extension } from '@codemirror/state';
import { classHighlighter } from '@lezer/highlight';
import type { SlowUpdateOptions } from '../interfaces/slow-update';
import type { Connection } from '../protocol/connection';
import type { Language } from '../protocol/languages';
import { lineSeparator } from '../protocol/line-separator';
import type { Session } from '../protocol/session';
import { keymaps } from './keymaps';
import { languageExtensions } from './languages';
import { autocompletionFromServer } from './server/autocompletion';
import { connectionState } from './server/connection-state';
import { infotipsFromServer } from './server/infotips';
import { lintingFromServer } from './server/linting';
import { sendChangesToServer } from './server/send-changes';
import { signatureHelpFromServer } from './server/signature-help';

export const createExtensions = <O, U>(
    connection: Connection<O, U>,
    session: Session<O>,
    options: { initialLanguage: Language } & SlowUpdateOptions<U>
): ReadonlyArray<Extension> => [
    indentUnit.of('    '),
    EditorState.lineSeparator.of(lineSeparator),

    history(),

    languageExtensions[options.initialLanguage],
    syntaxHighlighting(classHighlighter),

    connectionState(connection),
    sendChangesToServer(session as Session),
    lintingFromServer(connection as Connection<unknown, U>, options),
    infotipsFromServer(connection),
    signatureHelpFromServer(connection as Connection),
    autocompletionFromServer(connection),

    // has to go last so that more specific keymaps
    // in e.g. autocomplete have more priority
    keymaps
];

export const createState = (
    extensions: ReadonlyArray<Extension>,
    options: {
        initialText?: string;
        initialCursorOffset?: number;
    } = {}
) => {
    return EditorState.create({
        ...(options.initialText ? { doc: options.initialText } : {}),
        ...(options.initialCursorOffset ? { selection: EditorSelection.single(options.initialCursorOffset) } : {}),
        extensions
    });
};