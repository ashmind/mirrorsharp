import type { Language, DiagnosticData, ServerOptions } from './protocol';

export interface EditorOptions<TExtensionData> {
    on?: {
        slowUpdateWait?: () => void;
        slowUpdateResult?: (args: { diagnostics: ReadonlyArray<DiagnosticData>; x: TExtensionData }) => void;
        textChange?: (getText: () => string) => void;
        connectionChange?: {
            (event: 'open', e: Event): void;
            (event: 'error', e: ErrorEvent): void;
            (event: 'close', e: CloseEvent): void;
        };
        serverError?: (message: string) => void;
    };
    forCodeMirror?: CodeMirror.EditorConfiguration;
    language?: Language;
}

export interface Editor<TServerOptions extends ServerOptions> {
    getCodeMirror(): CodeMirror.Editor;
    setText(text: string): void;
    getLanguage(): Language;
    setLanguage(value: Language): void;
    sendServerOptions(value: TServerOptions): Promise<void>;
    destroy(destroyOptions?: DestroyOptions): void;
}

export interface DestroyOptions {
    readonly keepCodeMirror?: boolean;
}