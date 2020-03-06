declare module NodeJS  {
    interface Global {
        document: {
            body: {
                createTextRange: () => ({
                    getBoundingClientRect(): void;
                    getClientRects(): ReadonlyArray<any>;
                })
            }
        };

        WebSocket: () => Partial<WebSocket>;
    }
}