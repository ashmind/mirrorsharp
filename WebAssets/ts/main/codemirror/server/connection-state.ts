import type { Connection } from '../../connection';
import { ViewPlugin } from '@codemirror/view';
import { addEvents } from '../../../helpers/add-events';
import { defineEffectField } from '../../../helpers/define-effect-field';

const [isConnected, dispatchIsConnectedChanged] = defineEffectField(false);

export { isConnected };

export const connectionState = <O, TExtensionData>(
    connection: Connection<O, TExtensionData>
) => {
    return [isConnected, ViewPlugin.define(view => {
        const removeEvents = addEvents(connection, {
            open: () => dispatchIsConnectedChanged(view, true),
            error: () => dispatchIsConnectedChanged(view, false),
            close: () => dispatchIsConnectedChanged(view, false)
        });

        return { destroy: () => removeEvents() };
    })];
};