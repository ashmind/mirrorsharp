declare namespace NodeJS  {
    interface Global {
        document: {
            body: {
                createTextRange: () => ({
                    getBoundingClientRect(): void;
                    getClientRects(): [];
                });
            };
        };

        WebSocket: () => Partial<WebSocket>;
    }
}