import type { Language, DiagnosticData } from './protocol';

export interface EditorOptions<TExtensionServerOptions, TSlowUpdateExtensionData> {
    language?: Language;
    on?: {
        slowUpdateWait?: () => void;
        slowUpdateResult?: (args: { diagnostics: ReadonlyArray<DiagnosticData>; x: TSlowUpdateExtensionData }) => void;
        textChange?: (getText: () => string) => void;
        connectionChange?: {
            (event: 'open', e: Event): void;
            (event: 'error', e: ErrorEvent): void;
            (event: 'close', e: CloseEvent): void;
        };
        serverError?: (message: string) => void;
    };
    forCodeMirror?: CodeMirror.EditorConfiguration;
    initialServerOptions?: TExtensionServerOptions;
}

export interface Editor<TExtensionServerOptions> {
    getCodeMirror(): CodeMirror.Editor;
    setText(text: string): void;
    getLanguage(): Language;
    setLanguage(value: Language): Promise<void>;
    setServerOptions(value: Partial<TExtensionServerOptions>): Promise<void>;
    destroy(destroyOptions?: DestroyOptions): void;
}

export interface DestroyOptions {
    readonly keepCodeMirror?: boolean;
}