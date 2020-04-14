import type { Connection } from './connection';
import type { SelfDebugMessage } from './protocol';

export interface SelfDebug<TExtensionServerOptions, TSlowUpdateExtensionData> {
    watchEditor(
        getText: () => string,
        getCursorIndex: () => number
    ): void;
    log(event: string, message: string): void;
    requestData(connection: Connection<TExtensionServerOptions, TSlowUpdateExtensionData>): Promise<void>;
    displayData(serverData: SelfDebugMessage): void;
}