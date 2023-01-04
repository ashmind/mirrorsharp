import type { EditorView } from '@codemirror/view';
import type { Language, DiagnosticSeverity } from './interfaces/protocol';
// import { SelfDebug } from './main/self-debug';
import { Connection } from './main/connection';
import { Editor } from './main/editor';
import { Session } from './main/session';

export type MirrorSharpLanguage = Language;
export type MirrorSharpConnectionState = 'open'|'error'|'close';

export interface MirrorSharpDiagnostic {
    readonly id: string;
    readonly severity: DiagnosticSeverity;
    readonly message: string;
}

export interface MirrorSharpSlowUpdateResult<TExtensionData = never> {
    readonly diagnostics: ReadonlyArray<MirrorSharpDiagnostic>;
    readonly x: TExtensionData;
}

export interface MirrorSharpOptions<TExtensionServerOptions = never, TSlowUpdateExtensionData = never> {
    readonly serviceUrl: string;

    readonly selfDebugEnabled?: boolean;
    readonly language?: MirrorSharpLanguage;
    readonly initialText?: string;
    readonly initialCursorOffset?: number;

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
    };

    readonly noInitialConnection?: boolean;
    readonly initialServerOptions?: TExtensionServerOptions;
}

export interface MirrorSharpInstance<TExtensionServerOptions> {
    getCodeMirrorView(): EditorView;
    getText(): string;
    getCursorOffset(): number;
    setText(text: string): void;
    getLanguage(): MirrorSharpLanguage;
    setLanguage(value: MirrorSharpLanguage): void;
    setServerOptions(value: TExtensionServerOptions): void;
    connect(): void;
    destroy(destroyOptions: { keepCodeMirror?: boolean }): void;
}

export default function mirrorsharp<TExtensionServerOptions = never, TSlowUpdateExtensionData = never>(
    container: HTMLElement,
    options: MirrorSharpOptions<TExtensionServerOptions, TSlowUpdateExtensionData>
): MirrorSharpInstance<TExtensionServerOptions> {
    // const selfDebug = options.selfDebugEnabled ? new SelfDebug() : null;
    const connection = new Connection<TExtensionServerOptions, TSlowUpdateExtensionData>(options.serviceUrl/*, selfDebug, */, { delayedOpen: options.noInitialConnection });
    const session = new Session<TExtensionServerOptions>(connection as Connection<TExtensionServerOptions>);
    const editor = new Editor(container, connection, session/*, selfDebug*/, options);

    let connectCalled = false;
    return Object.freeze({
        getCodeMirrorView: () => editor.getCodeMirrorView(),
        getText: () => editor.getText(),
        getCursorOffset: () => editor.getCursorOffset(),
        setText: (text: string) => editor.setText(text),
        getLanguage: () => editor.getLanguage(),
        setLanguage: (value: Language) => editor.setLanguage(value),
        setServerOptions: (value: TExtensionServerOptions) => editor.setServerOptions(value),
        connect: () => {
            if (!options.noInitialConnection)
                throw new Error('Connect can only be called if options.noInitialConnection was set.');
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