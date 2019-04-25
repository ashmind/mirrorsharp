declare namespace internal {
    export interface EventSource {
        on(event: string, handler: Function): void;
        off(event: string, handler: Function): void;
    }

    export interface Options extends public.Options {
        selfDebugEnabled?: boolean;
    }

    export type SpanData = public.SpanData;

    export interface Range {
        readonly from: CodeMirror.Pos;
        readonly to: CodeMirror.Pos;
    }
}