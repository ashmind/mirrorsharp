declare module CodeMirror {
    export interface Editor {
        setOption(name: 'lintFix', value: LintFixOptions): void;
    }

    export interface LintFixOptions {
        getFixes: (cm: Editor, line: number, annotations: ReadonlyArray<LintAnnotation>) => ReadonlyArray<LintFix>
    }

    interface EditorConfiguration {
        lintFix?: LintFixOptions;
    }

    export interface LintFix {
        readonly text: string;
        readonly apply: (cm: Editor, line: number, fix: LintFix) => void;
    }
}