interface MockSocketMessageEvent {
    readonly data: string;
}

type MockSocketListenerMap = {
    open: () => void;
    message: (e: MockSocketMessageEvent) => void;
    error: () => void;
    close: () => void;
};

export class MockSocketController {
    readonly #listeners = {
        open: [],
        message: [],
        error: [],
        close: []
    } as {
        [K in keyof MockSocketListenerMap]: Array<MockSocketListenerMap[K]>
    };

    readyState = MockSocket.CONNECTING as (
        typeof MockSocket.CONNECTING
        | typeof MockSocket.OPEN
        | typeof MockSocket.CLOSING
        | typeof MockSocket.CLOSED
    );

    url?: string;
    createdCount = 0;
    sent = [] as Array<string>;

    open({ asyncEvents }: { asyncEvents?: boolean | undefined } = {}) {
        this.readyState = MockSocket.OPEN;

        const triggerEvent = () => {
            for (const listener of this.#listeners['open']) {
                listener();
            }
        };
        // Required when called from constructor, otherwise
        // the event is triggered before the listener is added
        if (asyncEvents) {
            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            new Promise<void>(resolve => resolve())
                .then(() => triggerEvent());
        }
        else {
            triggerEvent();
        }
    }

    receive(e: MockSocketMessageEvent) {
        for (const listener of this.#listeners['message']) {
            listener(e);
        }
    }

    close() {
        this.readyState = MockSocket.CLOSED;
        for (const listener of this.#listeners['close']) {
            listener();
        }
    }

    addEventListener<TEvent extends keyof MockSocketListenerMap>(event: TEvent, listener: MockSocketListenerMap[TEvent]) {
        this.#listeners[event].push(listener);
    }

    removeEventListener<TEvent extends keyof MockSocketListenerMap>(event: TEvent, listener: MockSocketListenerMap[TEvent]) {
        const index = this.#listeners[event].indexOf(listener);
        if (index > 0)
            this.#listeners[event].splice(index, 1);
    }
}

export class MockSocket {
    static readonly CONNECTING = 0;
    static readonly OPEN = 1;
    static readonly CLOSING = 2;
    static readonly CLOSED = 3;

    readonly mock = new MockSocketController();

    get url() {
        return this.mock.url;
    }

    get readyState() {
        return this.mock.readyState;
    }

    send(message: string) {
        this.mock.sent.push(message);
    }

    addEventListener<TEvent extends keyof MockSocketListenerMap>(event: TEvent, listener: MockSocketListenerMap[TEvent]) {
        this.mock.addEventListener(event, listener);
    }

    removeEventListener<TEvent extends keyof MockSocketListenerMap>(event: TEvent, listener: MockSocketListenerMap[TEvent]) {
        this.mock.removeEventListener(event, listener);
    }

    close() {
        this.mock.close();
    }
}

export const installMockSocket = (socket: MockSocket, { manualOpen }: { manualOpen?: boolean | undefined } = {}) => {
    if (globalThis.WebSocket instanceof MockSocket)
        throw new Error(`Global WebSocket is already set up in this context.`);

    // eslint-disable-next-line func-style
    const WebSocket = function(url: string) {
        socket.mock.url = url;
        socket.mock.createdCount += 1;
        if (!manualOpen && socket.readyState !== MockSocket.OPEN)
            socket.mock.open({ asyncEvents: true });
        return socket;
    };
    WebSocket.CONNECTING = MockSocket.CONNECTING;
    WebSocket.OPEN = MockSocket.OPEN;
    WebSocket.CLOSING = MockSocket.CLOSING;
    WebSocket.CLOSED = MockSocket.CLOSED;

    (globalThis as unknown as { WebSocket: (url: string) => Partial<WebSocket> }).WebSocket = WebSocket;
    return socket;
};