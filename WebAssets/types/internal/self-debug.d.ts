declare namespace internal {
    export interface SelfDebug {
        watchEditor(
            getText: () => string,
            getCursorIndex: () => number
        ): void;
        log(event: string, message: string): void;
        requestData(connection: Connection): void;
        displayData(serverData: SelfDebugMessage): void;
    }
}