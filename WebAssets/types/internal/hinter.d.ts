declare namespace internal {
    export interface Hinter {
        start(list: ReadonlyArray<internal.CompletionItemData>, span: SpanData, options: CompletionOptionalData): void;
        showTip(index: number, parts: ReadonlyArray<PartData>): void;
        destroy(): void;
    }

    export interface HinterCompatibility {
        disableItemInfo?: boolean;
    }

    export interface HintsResultEx extends CodeMirror.HintsResult {
        list: ReadonlyArray<HintEx>
    }

    export interface HintEx extends CodeMirror.Hint {
        ['$mirrorsharp-indexInList']?: number;
        ['$mirrorsharp-priority']?: number;
        ['$mirrorsharp-cached-info']?: ReadonlyArray<internal.PartData>;
    }

    export interface CompletionOptionalData {
        readonly suggestion?: CompletionSuggestionData
        readonly commitChars: ReadonlyArray<string>
    }
}