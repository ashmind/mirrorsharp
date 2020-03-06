import type { PartData, SpanData, CompletionItemData, CompletionSuggestionData } from './protocol';

export interface Hinter {
    start(list: ReadonlyArray<CompletionItemData>, span: SpanData, options: CompletionOptionalData): void;
    showTip(index: number, parts: ReadonlyArray<PartData>): void;
    destroy(): void;
}

export interface HintsResult {
    list: ReadonlyArray<Hint>;
}

export interface Hint {
    text?: string;
    displayText?: string;
    from?: CodeMirror.Position;
    className?: string;
    hint?: () => void;

    ['$mirrorsharp-indexInList']?: number;
    ['$mirrorsharp-priority']?: number;
    ['$mirrorsharp-cached-info']?: ReadonlyArray<PartData>;
}

export interface CompletionOptionalData {
    readonly suggestion?: CompletionSuggestionData;
    readonly commitChars: ReadonlyArray<string>;
}