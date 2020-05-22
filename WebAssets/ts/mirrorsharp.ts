import type { EditorView } from '@codemirror/next/view';
import type { Language, DiagnosticSeverity } from './interfaces/protocol';
// import { SelfDebug } from './main/self-debug';
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

    readonly initialServerOptions?: TExtensionServerOptions;
}

export interface MirrorSharpInstance<TExtensionServerOptions> {
    getCodeMirrorView(): EditorView;
    getText(): string;
    // setText(text: string): void;
    getLanguage(): MirrorSharpLanguage;
    setLanguage(value: MirrorSharpLanguage): void;
    setServerOptions(value: TExtensionServerOptions): Promise<void>;
    destroy(destroyOptions: { keepCodeMirror?: boolean }): void;
}

export default function mirrorsharp<TExtensionServerOptions = never, TSlowUpdateExtensionData = never>(
    container: HTMLElement,
    options: MirrorSharpOptions<TExtensionServerOptions, TSlowUpdateExtensionData>
): MirrorSharpInstance<TExtensionServerOptions> {
    // const selfDebug = options.selfDebugEnabled ? new SelfDebug() : null;
    const connection = new Connection<TExtensionServerOptions, TSlowUpdateExtensionData>(options.serviceUrl/*, selfDebug*/);
    const editor = new Editor(container, connection/*, selfDebug*/, options);

    return Object.freeze({
        getCodeMirrorView: () => editor.getCodeMirrorView(),
        getText: () => editor.getText(),
        // setText: (text: string) => editor.setText(text),
        getLanguage: () => editor.getLanguage(),
        setLanguage: (value: Language) => editor.setLanguage(value),
        setServerOptions: (value: TExtensionServerOptions) => editor.setServerOptions(value),

        destroy(destroyOptions?: { keepCodeMirror?: boolean }) {
            editor.destroy(destroyOptions);
            connection.close();
        }
    });
}