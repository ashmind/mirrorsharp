import type { Extension } from '@codemirror/state';
import type { EditorView } from '@codemirror/view';
import { omit } from './helpers/omit';
import { validateOptionKeys } from './helpers/validate-option-keys';
import type { Theme } from './interfaces/theme';
import { Editor } from './main/editor';
import { Connection } from './protocol/connection';
import type { Language } from './protocol/languages';
import type { DiagnosticSeverity } from './protocol/messages';
import { Session } from './protocol/session';

// ts-unused-exports:disable-next-line
export type MirrorSharpLanguage = Language;
// ts-unused-exports:disable-next-line
export type MirrorSharpConnectionState = 'open' | 'error' | 'close';

// ts-unused-exports:disable-next-line
export type MirrorSharpTheme = Theme;

// ts-unused-exports:disable-next-line
export interface MirrorSharpDiagnostic {
    readonly id: string;
    readonly severity: DiagnosticSeverity;
    readonly message: string;
}

// ts-unused-exports:disable-next-line
export type MirrorSharpSlowUpdateResult<TExtensionData = void> = void extends TExtensionData ? {
    readonly diagnostics: ReadonlyArray<MirrorSharpDiagnostic>
} : {
    readonly diagnostics: ReadonlyArray<MirrorSharpDiagnostic>;
    readonly x: TExtensionData;
};

// ts-unused-exports:disable-next-line
export type MirrorSharpOptions<TExtensionServerOptions = void, TSlowUpdateExtensionData = void> = {
    readonly serviceUrl: string;

    readonly language?: MirrorSharpLanguage | undefined;
    readonly theme?: MirrorSharpTheme | undefined;
    readonly text?: string | undefined;
    readonly cursorOffset?: number | undefined;

    // See EditorOptions<TExtensionData>['on']. This is not DRY, but
    // it's good to be explicit on what we are exporting.
    readonly on?: {
        readonly slowUpdateWait?: () => void;
        readonly slowUpdateResult?: (args: MirrorSharpSlowUpdateResult<TSlowUpdateExtensionData>) => void;
        readonly textChange?: (getText: () => string) => void;
        readonly connectionChange?: {
            (event: 'open', e: Event): void;
            (event: 'error', e: ErrorEvent): void;
            (event: 'close', e: CloseEvent): void;
        };
        readonly serverError?: (message: string) => void;
    } | undefined;

    readonly disconnected?: boolean | undefined;
    readonly serverOptions?: TExtensionServerOptions | undefined;

    readonly codeMirror?: {
        extensions?: ReadonlyArray<Extension>;
    }
};

// ts-unused-exports:disable-next-line
export interface MirrorSharpInstance<TExtensionServerOptions> {
    getCodeMirrorView(): EditorView;
    getRootElement(): Element;
    getText(): string;
    setText(text: string): void;
    getCursorOffset(): number;
    getLanguage(): MirrorSharpLanguage;
    setLanguage(value: MirrorSharpLanguage): void;
    setServerOptions(value: TExtensionServerOptions): void;
    setTheme(value: MirrorSharpTheme): void;
    connect(): void;
    destroy(destroyOptions: { keepCodeMirror?: boolean }): void;
}

// ts-unused-exports:disable-next-line
export
// eslint-disable-next-line import/no-default-export
default function mirrorsharp<TExtensionServerOptions = void, TSlowUpdateExtensionData = void>(
    container: HTMLElement,
    options: MirrorSharpOptions<TExtensionServerOptions, TSlowUpdateExtensionData>
): MirrorSharpInstance<TExtensionServerOptions> {
    validateOptionKeys(options, [
        'serviceUrl',
        'language',
        'text',
        'cursorOffset',
        'theme',
        'serverOptions',
        'on',
        'codeMirror',
        'disconnected'
    ]);
    validateOptionKeys(options.on, [
        'textChange',
        'connectionChange',
        'serverError',
        'slowUpdateWait',
        'slowUpdateResult'
    ], 'on');
    validateOptionKeys(options.codeMirror, ['extensions'], 'codeMirror');

    const connection = new Connection<TExtensionServerOptions, TSlowUpdateExtensionData>(options.serviceUrl, { delayedOpen: options.disconnected });
    const session = new Session<TExtensionServerOptions>(connection as Connection<TExtensionServerOptions>);
    const editor = new Editor(container, connection, session, omit(options, ['serviceUrl']));

    let connectCalled = false;
    return Object.freeze({
        getCodeMirrorView: () => editor.getCodeMirrorView(),
        getRootElement: () => editor.getRootElement(),
        getText: () => editor.getText(),
        setText: (text: string) => editor.setText(text),
        getCursorOffset: () => editor.getCursorOffset(),
        getLanguage: () => editor.getLanguage(),
        setLanguage: (value: Language) => editor.setLanguage(value),
        setServerOptions: (value: TExtensionServerOptions) => editor.setServerOptions(value),
        setTheme: (theme: MirrorSharpTheme) => editor.setTheme(theme),

        connect: () => {
            if (!options.disconnected)
                throw new Error('Connect can only be called if options.disconnected was set.');
            if (connectCalled)
                throw new Error('Connect can only be called once per mirrorsharp instance (on start).');
            connection.open();
            connectCalled = true;
        },

        destroy(destroyOptions?: { keepCodeMirror?: boolean }) {
            editor.destroy(destroyOptions);
            session.destroy();
            connection.close();
        }
    });
}