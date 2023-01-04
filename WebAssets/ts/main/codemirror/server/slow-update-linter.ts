import { ViewPlugin, EditorView, PluginValue } from '@codemirror/view';
import { Diagnostic, setDiagnostics } from '@codemirror/lint';
import type { Connection } from '../../connection';
import type { SlowUpdateOptions } from '../../../interfaces/slow-update';
import type { DiagnosticData } from '../../../interfaces/protocol';
import { addEvents } from '../../../helpers/add-events';

const mapDiagnostic = ({ span: { start, length }, message, severity }: DiagnosticData): Diagnostic => ({
    from: start,
    to: start + length,
    message,
    severity: severity as 'error'|'info'|'warning'
});

const createLinterPlugin = <O, TExtensionData>(
    view: EditorView,
    connection: Connection<O, TExtensionData>,
    { slowUpdateResult }: SlowUpdateOptions<TExtensionData>
) => {
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
                slowUpdateResult(message);
        }
    });

    return {
        destroy() {
            removeConnectionEvents();
        }
    } as PluginValue;
};

export const slowUpdateLinter = <O, TExtensionData>(
    connection: Connection<O, TExtensionData>,
    options: SlowUpdateOptions<TExtensionData>
) => [
    ViewPlugin.define(view => createLinterPlugin(view, connection, options))
];