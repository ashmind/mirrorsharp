import type { Connection } from './connection';
import type { SelfDebugMessage } from './protocol';

export interface SelfDebug<TExtensionData> {
    watchEditor(
        getText: () => string,
        getCursorIndex: () => number
    ): void;
    log(event: string, message: string): void;
    requestData(connection: Connection<TExtensionData>): void;
    displayData(serverData: SelfDebugMessage): void;
}