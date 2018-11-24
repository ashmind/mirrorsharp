declare namespace internal {
    export interface SignatureTip {
        update(signatures: ReadonlyArray<SignatureData>, span: SpanData): void;
        hide(): void;
    }

    export interface SignatureData {
        readonly parts: ReadonlyArray<SignaturePartData>;
        readonly selected: boolean;
    }

    export interface SignaturePartData extends PartData {
        readonly selected: boolean;
    }
}