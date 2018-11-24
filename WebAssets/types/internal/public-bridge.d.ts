import * as _public from '../public';

declare global {
    namespace public {
        export type Instance = _public.Instance;
        export type Options = _public.Options;
        export type Language = _public.Language;
        export type SpanData = _public.SpanData;
        export type DiagnosticData = _public.DiagnosticData;
        export type DiagnosticSeverity = _public.DiagnosticSeverity;
        export type ServerOptions = _public.ServerOptions;
        export type DestroyOptions = _public.DestroyOptions;
    }
}