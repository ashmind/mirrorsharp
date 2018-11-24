declare namespace internal {
    export interface EditorOptions {
        on?: {
            slowUpdateWait: () => void;
            slowUpdateResult: (args: { diagnostics: ReadonlyArray<DiagnosticData>, x: any }) => void;
            textChange: Function;
            connectionChange: Function;
            serverError: (message: string) => void;
        };
        forCodeMirror?: CodeMirror.EditorConfiguration;
        language?: public.Language;
        sharplabPreQuickInfoCompatibilityMode?: boolean;
    }
}