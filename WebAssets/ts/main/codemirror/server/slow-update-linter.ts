import { ViewPlugin, EditorView, PluginValue } from '@codemirror/view';
import { Diagnostic, setDiagnostics } from '@codemirror/lint';
import type { Connection } from '../../connection';
import type { SlowUpdateOptions } from '../../../interfaces/slow-update';
import type { DiagnosticData } from '../../../interfaces/protocol';
import { isConnected } from './connection-state';
import { addEvents } from '../../../helpers/add-events';

const timeout = 500;

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
    let suspended = !view.state.field(isConnected);
    let hadText = false;
    let timer = null as ReturnType<typeof setTimeout>|null;

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
        update({ docChanged, state }) {
            const shouldSkipChange = timer || (state.doc.length === 0 && !hadText);

            if (!state.field(isConnected)) {
                suspended = true;
                if (timer) {
                    clearTimeout(timer);
                    timer = null;
                }
                return;
            }
            else if (suspended) {
                suspended = false;
                // if doc changed we will send an update below anyways
                if (!docChanged && !shouldSkipChange) {
                    // eslint-disable-next-line @typescript-eslint/no-floating-promises
                    connection.sendSlowUpdate();
                    return;
                }
            }

            if (!docChanged || shouldSkipChange)
                return;

            hadText = true;
            timer = setTimeout(() => {
                // eslint-disable-next-line @typescript-eslint/no-floating-promises
                connection.sendSlowUpdate();
                timer = null;
            }, timeout);
        },

        destroy() {
            if (timer)
                clearTimeout(timer);
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