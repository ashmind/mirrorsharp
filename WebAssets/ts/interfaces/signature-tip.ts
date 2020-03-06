import { SpanData, SignatureData } from './protocol';

export interface SignatureTip {
    update(signatures: ReadonlyArray<SignatureData>, span: SpanData): void;
    hide(): void;
}