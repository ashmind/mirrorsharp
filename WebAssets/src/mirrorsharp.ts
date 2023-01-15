import type { Extension } from '@codemirror/state';
import type { EditorView } from '@codemirror/view';
import type { StyleSpec } from 'style-mod';
import { createInstance } from './main/instance';
import type { Theme } from './main/theme';
import type { Language } from './protocol/languages';
import type { DiagnosticSeverity } from './protocol/messages';

// ts-unused-exports:disable-next-line
export type MirrorSharpDiagnosticSeverity = DiagnosticSeverity;

// ts-unused-exports:disable-next-line
export type MirrorSharpLanguage = Language;
// ts-unused-exports:disable-next-line
export type MirrorSharpConnectionState = 'open' | 'error' | 'close';

// ts-unused-exports:disable-next-line
export type MirrorSharpTheme = Theme;

// ts-unused-exports:disable-next-line
export interface MirrorSharpDiagnostic {
    readonly id: string;
    readonly severity: MirrorSharpDiagnosticSeverity;
    readonly message: string;
}

// ts-unused-exports:disable-next-line
export type MirrorSharpSlowUpdateResult<TExtensionData = void> = void extends TExtensionData ? {
    readonly diagnostics: ReadonlyArray<MirrorSharpDiagnostic>
} : {
    readonly diagnostics: ReadonlyArray<MirrorSharpDiagnostic>;
    readonly extensionResult: TExtensionData;
};

// ts-unused-exports:disable-next-line
export type MirrorSharpOptions<TExtensionServerOptions = void, TSlowUpdateExtensionData = void> = {
    readonly serviceUrl: string;

    readonly language?: MirrorSharpLanguage | undefined;
    readonly theme?: MirrorSharpTheme | undefined;
    readonly text?: string | undefined;
    readonly cursorOffset?: number | undefined;

    // See EditorOptions<TExtensionData>['on']. This is not DRY, but
    // it's good to be explicit on what we are exporting.
    readonly on?: {
        readonly slowUpdateWait?: () => void;
        readonly slowUpdateResult?: (result: MirrorSharpSlowUpdateResult<TSlowUpdateExtensionData>) => void;
        readonly textChange?: (getText: () => string) => void;
        readonly connectionChange?: (event: 'open' | 'loss') => void;
        readonly serverError?: (message: string) => void;
    } | undefined;

    readonly disconnected?: boolean | undefined;
    readonly serverOptions?: TExtensionServerOptions | undefined;

    readonly codeMirror?: {
        extensions?: ReadonlyArray<Extension>;
        theme?: { [selector: string]: StyleSpec; }
    }
};

// ts-unused-exports:disable-next-line
export interface MirrorSharpInstance<TExtensionServerOptions> {
    getCodeMirrorView(): EditorView;
    getRootElement(): Element;
    getText(): string;
    setText(text: string): void;
    getCursorOffset(): number;
    getLanguage(): MirrorSharpLanguage;
    setLanguage(value: MirrorSharpLanguage): void;
    setServerOptions(value: TExtensionServerOptions): void;
    setTheme(value: MirrorSharpTheme): void;
    setServiceUrl(url: string, options?: ({ disconnected?: boolean } | undefined)): void;
    connect(): void;
    destroy(): void;
}

const ensureNoUnknownOptions = <T extends Record<string, never>>(keyPrefix: string, unknown: T) => {
    let keys = Object.keys(unknown);
    if (keyPrefix)
        keys = keys.map(k => `${keyPrefix}.${k}`);
    if (keys.length === 0)
        return;

    throw new Error(`Unknown option${keys.length > 1 ? 's' : ''}: '${keys.join("', '")}'`);
};

// ts-unused-exports:disable-next-line
export
// eslint-disable-next-line import/no-default-export
default function mirrorsharp<TExtensionServerOptions = void, TSlowUpdateExtensionData = void>(
    container: HTMLElement,
    options: MirrorSharpOptions<TExtensionServerOptions, TSlowUpdateExtensionData>
): MirrorSharpInstance<TExtensionServerOptions> {
    const {
        serviceUrl,
        text,
        cursorOffset,
        language,
        theme,
        on,
        disconnected,
        serverOptions,
        codeMirror,
        ...rest
    } = options;
    ensureNoUnknownOptions('', rest);

    const {
        textChange,
        connectionChange,
        serverError,
        slowUpdateWait,
        slowUpdateResult,
        ...onRest
    } = on ?? {};
    ensureNoUnknownOptions('on', onRest);

    const {
        extensions,
        theme: themeSpec,
        ...codeMirrorRest
    } = codeMirror ?? {};
    ensureNoUnknownOptions('codeMirror', codeMirrorRest);

    const instance = createInstance<TExtensionServerOptions, TSlowUpdateExtensionData>(container, {
        serviceUrl,
        text,
        cursorOffset,
        language,
        theme,
        on: {
            textChange,
            connectionChange,
            serverError,
            slowUpdateWait,
            slowUpdateResult
        },
        serverOptions,
        disconnected,
        codeMirror: {
            extensions,
            theme: themeSpec
        }
    });

    return instance satisfies {
        // blocks any unexpected members from being exported to public API
        [K in Exclude<keyof typeof instance, keyof MirrorSharpInstance<unknown>>]: never
    };
}