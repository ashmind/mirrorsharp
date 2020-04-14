import type { SpanData, SignatureData } from './protocol';

export interface SignatureTip {
    update(data: { signatures: ReadonlyArray<SignatureData>; span: SpanData }|{ signatures?: undefined; span?: undefined }): void;
    hide(): void;
}