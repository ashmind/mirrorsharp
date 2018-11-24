declare namespace internal {
    export type StateCommand = 'cancel'|'force';

    export interface Connection extends EventSource {
        sendReplaceText(start: number, length: number, newText: string, cursorIndexAfter: number, reason?: string): Promise<void>;
        sendMoveCursor(cursorIndex: number): Promise<void>;
        sendTypeChar(char: string): Promise<void>;
        sendCompletionState(indexOrCommand: number|StateCommand): Promise<void>;
        sendCompletionState(command: 'info', index: number): Promise<void>;
        sendSignatureHelpState(indexOrCommand: StateCommand): Promise<void>;
        sendRequestInfoTip(cursorIndex: number): Promise<void>;
        sendSlowUpdate(): Promise<void>;
        sendApplyDiagnosticAction(actionId: number): Promise<void>;
        sendSetOptions(options: public.ServerOptions): Promise<void>;
        sendRequestSelfDebugData(): Promise<void>;
        close(): void;
    }
}