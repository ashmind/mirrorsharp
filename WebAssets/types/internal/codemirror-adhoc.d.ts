declare module CodeMirror {
    export interface LintState {
        waitingFor?: number
    }

    export interface Pos {
        ['$mirrorsharp-index']: number;
    }

    export interface LintAnnotation {
        diagnostic: internal.DiagnosticData
    }

    export interface LintFix {
        id: number
    }

    export function on(
        object: internal.HintsResultEx,
        type: 'select',
        func: (completion: internal.HintEx, element: HTMLElement) => void
    ) : void;
}