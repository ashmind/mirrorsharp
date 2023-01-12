import { history } from '@codemirror/commands';
import { indentUnit, syntaxHighlighting } from '@codemirror/language';
import { EditorState, EditorSelection, Extension } from '@codemirror/state';
import { EditorView } from '@codemirror/view';
import { classHighlighter } from '@lezer/highlight';
import type { SlowUpdateOptions } from '../interfaces/slow-update';
import { Theme, THEME_DARK } from '../interfaces/theme';
import type { Connection } from '../protocol/connection';
import type { Language } from '../protocol/languages';
import { lineSeparator } from '../protocol/line-separator';
import type { Session } from '../protocol/session';
import { switchableExtension } from './helpers/switchableExtension';
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
    options: {
        initialLanguage: Language;
        theme: Theme;
    } & SlowUpdateOptions<U>
) => {
    const language = switchableExtension(options.initialLanguage, l => languageExtensions[l]);
    const theme = switchableExtension(options.theme, t => EditorView.theme({}, { dark: t === THEME_DARK }));
    const initialExtensions = [
        indentUnit.of('    '),
        EditorState.lineSeparator.of(lineSeparator),

        history(),

        language.extension,
        syntaxHighlighting(classHighlighter),

        connectionState(connection),
        sendChangesToServer(session as Session),
        lintingFromServer(connection as Connection<unknown, U>, options),
        infotipsFromServer(connection),
        signatureHelpFromServer(connection as Connection),
        autocompletionFromServer(connection),

        // has to go last so that more specific keymaps
        // in e.g. autocomplete have more priority
        keymaps,

        theme.extension
    ];

    return [
        initialExtensions,
        {
            switchLanguageExtension: language.switch,
            switchThemeExtension: theme.switch
        }
    ] as const;
};

export type ExtensionSwitcher = ReturnType<typeof createExtensions>[1];

export const createState = (
    extensions: ReadonlyArray<Extension>,
    options: {
        text?: string | undefined;
        cursorOffset?: number | undefined;
    } = {}
) => {
    return EditorState.create({
        ...(options.text ? { doc: options.text } : {}),
        ...(options.cursorOffset ? { selection: EditorSelection.single(options.cursorOffset) } : {}),
        extensions
    });
};