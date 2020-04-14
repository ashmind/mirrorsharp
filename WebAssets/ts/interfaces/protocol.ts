export type Message<TExtensionServerOptions, TSlowUpdateExtensionData> = ChangesMessage
                    | CompletionsMessage
                    | CompletionInfoMessage
                    | SignaturesMessage
                    | SignaturesEmptyMessage
                    | InfotipMessage
                    | InfotipEmptyMessage
                    | SlowUpdateMessage<TSlowUpdateExtensionData>
                    | OptionsEchoMessage<TExtensionServerOptions>
                    | SelfDebugMessage
                    | ErrorMessage
                    | UnknownMessage;

export interface ChangesMessage {
    readonly type: 'changes';
    readonly changes: ReadonlyArray<ChangeData>;
    readonly reason: 'completion'|'fix';
}

export interface ChangeData {
    readonly start: number;
    readonly length: number;
    readonly text: string;
}

export interface CompletionsMessage {
    readonly type: 'completions';
    readonly span: SpanData;
    readonly completions: ReadonlyArray<CompletionItemData>;
    readonly commitChars: ReadonlyArray<string>;
    readonly suggestion?: CompletionSuggestionData;
}

export interface CompletionSuggestionData {
    readonly displayText: string;
}

export interface CompletionItemData {
    readonly filterText: string;
    readonly displayText: string;
    readonly priority: number;
    readonly kinds: ReadonlyArray<string>;
    readonly span?: SpanData;
}

export interface CompletionInfoMessage {
    readonly type: 'completionInfo';
    readonly index: number;
    readonly parts: ReadonlyArray<PartData>;
}

export interface SignaturesMessage {
    readonly type: 'signatures';
    readonly span: SpanData;
    readonly signatures: ReadonlyArray<SignatureData>;
}

export interface SignaturesEmptyMessage {
    readonly type: 'signatures';
    readonly span?: undefined;
    readonly signatures?: undefined;
}

export interface SignatureData {
    readonly parts: ReadonlyArray<SignaturePartData>;
    readonly selected?: boolean;
}

export interface SignaturePartData extends PartData {
    readonly selected?: boolean;
}

export interface InfotipMessage {
    readonly type: 'infotip';
    readonly span: SpanData;
    readonly kinds: ReadonlyArray<string>;
    readonly sections: ReadonlyArray<InfotipSectionData>;
}

export interface InfotipEmptyMessage {
    readonly type: 'infotip';
    readonly sections?: undefined;
}

export interface InfotipSectionData {
    readonly kind: string;
    readonly parts: ReadonlyArray<PartData>;
}

export interface SlowUpdateMessage<TExtensionData> {
    readonly type: 'slowUpdate';
    readonly diagnostics: ReadonlyArray<DiagnosticData>;
    readonly x: TExtensionData;
}

export interface DiagnosticData {
    readonly id: string;
    readonly span: SpanData;
    readonly severity: DiagnosticSeverity;
    readonly message: string;
    readonly tags: ReadonlyArray<string>;
    readonly actions?: ReadonlyArray<DiagnosticActionData>;
}

export type DiagnosticSeverity = 'hidden'|'warning'|'error'|'info';

export interface DiagnosticActionData {
    readonly id: number;
    readonly title: string;
}

export interface OptionsEchoMessage<TExtensionServerOptions> {
    readonly type: 'optionsEcho';
    readonly options: ServerOptions & TExtensionServerOptions;
}

export type Language = 'C#'|'Visual Basic'|'F#'|'PHP';

export interface ServerOptions {
    language?: Language;
}

export interface SelfDebugMessage {
    readonly type: 'self:debug';
    readonly log: ReadonlyArray<SelfDebugLogEntryData>;
}

export interface SelfDebugLogEntryData {
    readonly time: Date;
    readonly event: string;
    readonly message: string;
    readonly text: string;
    readonly cursor: number;
}

export interface ErrorMessage {
    readonly type: 'error';
    readonly message: string;
}

export interface UnknownMessage {
    type: '_';
}

export interface PartData {
    readonly kind: string;
    readonly text: string;
}

export interface SpanData {
    readonly start: number;
    readonly length: number;
}

type DiscriminateUnion<T, K extends keyof T, V extends T[K]> =
  T extends Record<K, V> ? T : never;

type MapDiscriminatedUnion<T extends Record<K, string>, K extends keyof T> =
  { [V in T[K]]: DiscriminateUnion<T, K, V> };

export type MessageMap<TExtensionServerOptions, TSlowUpdateExtensionData> =
    MapDiscriminatedUnion<Message<TExtensionServerOptions, TSlowUpdateExtensionData>, 'type'>;