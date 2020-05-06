import type { Connection } from '../../connection';
import type { SlowUpdateOptions } from '../../../interfaces/slow-update';
import type { DiagnosticData } from '../../../interfaces/protocol';
import { linter, Diagnostic } from '@codemirror/next/lint';

export const slowUpdateLinter = <O, TExtensionData>(
    connection: Connection<O, TExtensionData>,
    { slowUpdateResult }: SlowUpdateOptions<TExtensionData>
) => {
    let resolveDiagnostics: ((diagnostics: ReadonlyArray<Diagnostic>) => void)|undefined;

    const mapDiagnostic = ({ span: { start, length }, message, severity }: DiagnosticData): Diagnostic => ({
        from: start,
        to: start + length,
        message,
        severity: severity as 'error'|'info'|'warning'
    });

    connection.on('message', message => {
        if (message.type !== 'slowUpdate')
            return;

        if (resolveDiagnostics) {
            const diagnostics = message.diagnostics.map(mapDiagnostic);
            diagnostics.sort((a, b) => {
                if (a.from > b.from) return 1;
                if (b.from > a.from) return -1;
                return 0;
            });
            resolveDiagnostics(diagnostics);
        }

        if (slowUpdateResult)
            slowUpdateResult(message);
    });

    return linter(async () => {
        const promise = new Promise<ReadonlyArray<Diagnostic>>(r => resolveDiagnostics = r);
        await connection.sendSlowUpdate();
        return promise;
    });
};