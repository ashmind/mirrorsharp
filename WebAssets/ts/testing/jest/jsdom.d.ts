declare namespace NodeJS  {
    interface Global {
        document: {
            body: {
                createTextRange: () => {
                    getBoundingClientRect(): void;
                    getClientRects(): [];
                };
            };

            getSelection: () => {
                readonly anchorNode: null;
                readonly anchorOffset: 0;
                readonly focusNode: null;
                readonly focusOffset: 0;
            };
        };

        WebSocket: () => Partial<WebSocket>;
    }
}