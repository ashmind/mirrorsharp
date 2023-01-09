interface MockSocketMessageEvent {
    readonly data: string;
}

type MockSocketHandlerMap = {
    open: () => void;
    message: (e: MockSocketMessageEvent) => void;
    error: () => void;
    close: () => void;
};

export class MockSocketController {
    readonly #handlers = {
        open: [],
        message: [],
        error: [],
        close: []
    } as {
        [K in keyof MockSocketHandlerMap]: Array<MockSocketHandlerMap[K]>
    };

    readyState = MockSocket.CONNECTING as (
        typeof MockSocket.CONNECTING
        | typeof MockSocket.OPEN
        | typeof MockSocket.CLOSING
        | typeof MockSocket.CLOSED
    );

    createdCount = 0;
    sent = [] as Array<string>;

    open() {
        this.readyState = MockSocket.OPEN;
        for (const handler of this.#handlers['open']) {
            handler();
        }
    }

    receive(e: MockSocketMessageEvent) {
        for (const handler of this.#handlers['message']) {
            handler(e);
        }
    }

    close() {
        this.readyState = MockSocket.CLOSED;
        for (const handler of this.#handlers['close']) {
            handler();
        }
    }

    addEventListener<TEvent extends keyof MockSocketHandlerMap>(event: TEvent, handler: MockSocketHandlerMap[TEvent]) {
        this.#handlers[event].push(handler);
    }
}

export class MockSocket {
    static readonly CONNECTING = 0;
    static readonly OPEN = 1;
    static readonly CLOSING = 2;
    static readonly CLOSED = 3;

    readonly mock = new MockSocketController();

    get readyState() {
        return this.mock.readyState;
    }

    send(message: string) {
        this.mock.sent.push(message);
    }

    addEventListener<TEvent extends keyof MockSocketHandlerMap>(event: TEvent, handler: MockSocketHandlerMap[TEvent]) {
        this.mock.addEventListener(event, handler);
    }
}

export const installMockSocket = (socket: MockSocket) => {
    if (globalThis.WebSocket instanceof MockSocket)
        throw new Error(`Global WebSocket is already set up in this context.`);

    // eslint-disable-next-line func-style
    const WebSocket = function() {
        socket.mock.createdCount += 1;
        return socket;
    };
    WebSocket.CONNECTING = MockSocket.CONNECTING;
    WebSocket.OPEN = MockSocket.OPEN;
    WebSocket.CLOSING = MockSocket.CLOSING;
    WebSocket.CLOSED = MockSocket.CLOSED;

    (globalThis as unknown as { WebSocket: () => Partial<WebSocket> }).WebSocket = WebSocket;
};