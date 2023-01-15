import { history } from '@codemirror/commands';
import { indentUnit, syntaxHighlighting } from '@codemirror/language';
import { EditorState, EditorSelection, Extension } from '@codemirror/state';
import { EditorView } from '@codemirror/view';
import { classHighlighter } from '@lezer/highlight';
import type { StyleSpec } from 'style-mod';
import { Theme, THEME_DARK } from '../main/theme';
import type { Connection } from '../protocol/connection';
import type { Language } from '../protocol/languages';
import { lineSeparator } from '../protocol/line-separator';
import type { Session } from '../protocol/session';
import { switchableExtension } from './helpers/switchableExtension';
import { keymaps } from './keymaps';
import { languageExtensions } from './languages';
import { notifyOnTextChanges } from './notify-on-text-changes';
import { autocompletionFromServer } from './server/autocompletion';
import { connectionState } from './server/connection-state';
import { diagnosticsFromServer } from './server/diagnostics';
import { infotipsFromServer } from './server/infotips';
import { sendChangesToServer } from './server/send-changes';
import { signatureHelpFromServer } from './server/signature-help';

export const createExtensions = <O, U>(
    connection: Connection<O, U>,
    session: Session<O, U>,
    options: {
        language: Language;
        theme: Theme;
        themeSpec: { [selector: string]: StyleSpec; };
        onTextChange: ((getText: () => string) => void) | undefined,
        extraExtensions: ReadonlyArray<Extension>;
    }
) => {
    const language = switchableExtension(options.language, l => languageExtensions[l]);
    const theme = switchableExtension(
        options.theme,
        t => EditorView.theme(options.themeSpec, { dark: t === THEME_DARK })
    );
    const initialExtensions = [
        indentUnit.of('    '),
        EditorState.lineSeparator.of(lineSeparator),

        history(),

        language.extension,
        syntaxHighlighting(classHighlighter),

        connectionState(connection),
        sendChangesToServer(session as Session),
        diagnosticsFromServer(connection as Connection<unknown, U>),
        infotipsFromServer(connection),
        signatureHelpFromServer(connection as Connection),
        autocompletionFromServer(connection),

        ...(options.onTextChange ? [notifyOnTextChanges(options.onTextChange)] : []),

        // has to go last so that more specific keymaps
        // in e.g. autocomplete have more priority
        keymaps,

        theme.extension
    ].concat(options.extraExtensions);

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