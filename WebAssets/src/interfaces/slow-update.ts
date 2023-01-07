import type { DiagnosticData } from './protocol';

export interface SlowUpdateOptions<TSlowUpdateExtensionData> {
    slowUpdateWait?: () => void;
    slowUpdateResult?: (args: { diagnostics: ReadonlyArray<DiagnosticData>; x: TSlowUpdateExtensionData }) => void;
}