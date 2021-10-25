import type { Language, DiagnosticSeverity } from './interfaces/protocol';
import { SelfDebug } from './main/self-debug';
import { Connection } from './main/connection';
import { Editor } from './main/editor';

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
    readonly forCodeMirror?: CodeMirror.EditorConfiguration;
}

export interface MirrorSharpInstance<TExtensionServerOptions> {
    getCodeMirror(): CodeMirror.Editor;
    setText(text: string): void;
    getLanguage(): MirrorSharpLanguage;
    setLanguage(value: MirrorSharpLanguage): void;
    setServerOptions(value: TExtensionServerOptions): Promise<void>;
    connect(): void;
    destroy(destroyOptions: { keepCodeMirror?: boolean }): void;
}

export default function mirrorsharp<TExtensionServerOptions = never, TSlowUpdateExtensionData = never>(
    textarea: HTMLTextAreaElement,
    options: MirrorSharpOptions<TExtensionServerOptions, TSlowUpdateExtensionData>
): MirrorSharpInstance<TExtensionServerOptions> {
    const selfDebug = options.selfDebugEnabled ? new SelfDebug() : null;
    const connection = new Connection<TExtensionServerOptions, TSlowUpdateExtensionData>(
        options.serviceUrl, selfDebug, { delayedOpen: options.noInitialConnection }
    );
    const editor = new Editor(textarea, connection, selfDebug, options);

    let connectCalled = false;
    return Object.freeze({
        getCodeMirror: () => editor.getCodeMirror(),
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
            connection.close();
        }
    });
}