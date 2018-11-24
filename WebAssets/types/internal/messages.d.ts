declare namespace internal {
    export type Message = ChangesMessage
                        | CompletionsMessage
                        | CompletionInfoMessage
                        | SignaturesMessage
                        | InfotipMessage
                        | SlowUpdateMessage
                        | OptionsEchoMessage
                        | SelfDebugMessage
                        | ErrorMessage
                        | UnknownMessage;

    export interface ChangesMessage {
        readonly type: 'changes';
        readonly changes: ReadonlyArray<ChangeData>;
        readonly reason: string;
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
        readonly kinds?: ReadonlyArray<string>;
        readonly tags?: ReadonlyArray<string>;
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

    export interface SignatureData {

    }

    export interface InfotipMessage {
        readonly type: 'infotip';
        readonly span: SpanData;
        readonly kinds: ReadonlyArray<string>;
        readonly sections: ReadonlyArray<InfotipSectionData>;
    }

    export interface InfotipSectionData {
        readonly kind: string;
        readonly parts: ReadonlyArray<PartData>;
    }

    export interface SlowUpdateMessage {
        readonly type: 'slowUpdate';
        readonly diagnostics: ReadonlyArray<DiagnosticData>;
        readonly x?: any;
    }

    export interface DiagnosticData extends public.DiagnosticData {
        readonly actions: ReadonlyArray<DiagnosticActionData>
    }

    export interface DiagnosticActionData {
        readonly id: number;
        readonly title: string;
    }

    export interface OptionsEchoMessage {
        readonly type: 'optionsEcho';
        readonly options: public.ServerOptions;
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
        readonly message: string
    }

    export interface UnknownMessage {
        type: '_'
    }

    export interface PartData {
        readonly kind: string;
        readonly text: string;
    }
}