import { ViewPlugin } from '@codemirror/view';
import { defineEffectField } from '../../helpers/define-effect-field';
import type { Connection } from '../../protocol/connection';

const [isConnected, dispatchIsConnectedChanged] = defineEffectField(false);

export const connectionState = <O, TExtensionData>(
    connection: Connection<O, TExtensionData>
) => {
    return [isConnected, ViewPlugin.define(view => {
        const removeListeners = connection.addEventListeners({
            open: () => dispatchIsConnectedChanged(view, true),
            close: () => dispatchIsConnectedChanged(view, false)
        });

        return { destroy: () => removeListeners() };
    })];
};