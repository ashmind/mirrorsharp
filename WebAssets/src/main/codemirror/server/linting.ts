import { ViewPlugin } from '@codemirror/view';
import { Action, Diagnostic, setDiagnostics, lintGutter } from '@codemirror/lint';
import type { Connection } from '../../connection';
import type { SlowUpdateOptions } from '../../../interfaces/slow-update';
import type { DiagnosticActionData, DiagnosticData, DiagnosticSeverity } from '../../../interfaces/protocol';
import { addEvents } from '../../../helpers/add-events';
import { applyChangesFromServer } from '../../../helpers/apply-changes-from-server';

const receiveSlowUpdateResultsFromServer = <TExtensionData>(
    connection: Connection<unknown, TExtensionData>,
    { slowUpdateResult }: SlowUpdateOptions<TExtensionData>
) => ViewPlugin.define(view => {
    const mapSeverity = (severity: DiagnosticSeverity, tags: ReadonlyArray<string>) => {
        if (severity === 'error' || severity === 'warning')
            return severity;

        if (tags.includes('unnecessary'))
            return 'unnecessary' as Diagnostic['severity'];

        return 'info';
    };

    const mapAction = ({ id, title }: DiagnosticActionData): Action => ({
        name: title,
        apply: () => {
            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            connection.sendApplyDiagnosticAction(id);
        }
    });

    const mapDiagnostic = ({ span: { start, length }, message, severity, actions, tags }: DiagnosticData): Diagnostic => ({
        from: start,
        to: start + length,
        message,
        severity: mapSeverity(severity, tags),
        actions: actions?.map(mapAction)
    });

    const removeConnectionEvents = addEvents(connection, {
        message(message) {
            if (message.type !== 'slowUpdate')
                return;

            const diagnostics = message.diagnostics.map(mapDiagnostic);
            diagnostics.sort((a, b) => {
                if (a.from > b.from) return 1;
                if (b.from > a.from) return -1;
                return 0;
            });
            view.dispatch(setDiagnostics(view.state, diagnostics));

            if (slowUpdateResult)
                slowUpdateResult({ diagnostics: message.diagnostics, x: message.x });
        }
    });

    return {
        destroy: () => removeConnectionEvents()
    };
});

const receiveFixChangesFromServer = <TExtensionData>(
    connection: Connection<unknown, TExtensionData>
) => ViewPlugin.define(view => {
    const removeConnectionEvents = addEvents(connection, {
        message(message) {
            if (message.type !== 'changes' || message.reason !== 'fix')
                return;

            applyChangesFromServer(view, message.changes);
        }
    });

    return {
        destroy: () => removeConnectionEvents()
    };
});

export const lintingFromServer = <TExtensionData>(
    connection: Connection<unknown, TExtensionData>,
    options: SlowUpdateOptions<TExtensionData>
) => [
    receiveSlowUpdateResultsFromServer(connection, options),
    receiveFixChangesFromServer(connection),
    lintGutter()
];