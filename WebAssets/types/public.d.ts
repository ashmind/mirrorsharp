export = mirrorsharp;

declare function mirrorsharp(
    textarea: HTMLTextAreaElement,
    options?: mirrorsharp.Options
): mirrorsharp.Instance;

declare namespace mirrorsharp {
    export type Language = 'C#'|'Visual Basic'|'F#'|'PHP';

    export interface Options {
        serviceUrl: string;
        on?: {
            slowUpdateWait: () => void;
            slowUpdateResult: (args: { diagnostics: ReadonlyArray<DiagnosticData>, x: any }) => void;
            textChange: Function;
            connectionChange: Function;
            serverError: (message: string) => void;
        };
        forCodeMirror?: CodeMirror.EditorConfiguration;
        language?: Language;
    }

    export interface Instance {
        getCodeMirror(): CodeMirror.Editor;
        setText(text: string): void;
        getLanguage(): Language;
        setLanguage(value: Language): void;
        sendServerOptions(value: ServerOptions): Promise<void>;
        destroy(destroyOptions: DestroyOptions): void;
    }

    export interface ServerOptions {
        [key: string]: string;
        language?: Language;
    }

    export interface DestroyOptions {
        readonly keepCodeMirror?: boolean;
    }

    export interface DiagnosticData {
        readonly span: SpanData;
        readonly severity: DiagnosticSeverity;
        readonly message: string;
        readonly tags: ReadonlyArray<string>;
    }

    export type DiagnosticSeverity = 'hidden'|'warning'|'error'|'info';

    export interface SpanData {
        readonly start: number;
        readonly length: number;
    }
}