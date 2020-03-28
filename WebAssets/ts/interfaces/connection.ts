import type { ServerOptions, Message } from './protocol';

export type StateCommand = 'cancel'|'force';

export type ConnectionOpenHandler = (e: Event) => void;
export type ConnectionMessageHandler<TExtensionData> = (data: Message<TExtensionData>, e: MessageEvent) => void;
export type ConnectionErrorHandler = (e: ErrorEvent) => void;
export type ConnectionCloseHandler = (e: CloseEvent) => void;

export type ConnectionEventMap<TExtensionData> = {
    open: ConnectionOpenHandler;
    message: ConnectionMessageHandler<TExtensionData>;
    error: ConnectionErrorHandler;
    close: ConnectionCloseHandler;
};

export interface Connection<TExtensionData> {
    on(key: 'open', handler: ConnectionOpenHandler): void;
    on(key: 'message', handler: ConnectionMessageHandler<TExtensionData>): void;
    on(key: 'error', handler: ConnectionErrorHandler): void;
    on(key: 'close', handler: ConnectionCloseHandler): void;

    off(key: 'open', handler: ConnectionOpenHandler): void;
    off(key: 'message', handler: ConnectionMessageHandler<TExtensionData>): void;
    off(key: 'error', handler: ConnectionErrorHandler): void;
    off(key: 'close', handler: ConnectionCloseHandler): void;

    sendReplaceText(start: number, length: number, newText: string, cursorIndexAfter: number, reason?: string|null): Promise<void>;
    sendMoveCursor(cursorIndex: number): Promise<void>;
    sendTypeChar(char: string): Promise<void>;
    sendCompletionState(indexOrCommand: number|StateCommand): Promise<void>;
    sendCompletionState(command: 'info', index: number): Promise<void>;
    sendSignatureHelpState(indexOrCommand: StateCommand): Promise<void>;
    sendRequestInfoTip(cursorIndex: number): Promise<void>;
    sendSlowUpdate(): Promise<void>;
    sendApplyDiagnosticAction(actionId: number): Promise<void>;
    sendSetOptions(options: ServerOptions): Promise<void>;
    sendRequestSelfDebugData(): Promise<void>;
    close(): void;
}