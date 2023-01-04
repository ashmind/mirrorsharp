import type { EditorView } from '@codemirror/view';
import { TransactionSpec, Transaction } from '@codemirror/state';
import type { PartData, CompletionItemData, ChangeData, ChangesMessage, CompletionsMessage, Message, InfotipMessage } from '../interfaces/protocol';
import mirrorsharp, { MirrorSharpOptions, MirrorSharpInstance } from '../mirrorsharp';

type TestRecorderOptions = { exclude?: (object: object, action: string) => boolean };

class TestRecorder {
    readonly #objects = new Map<string, Record<string, unknown>>();
    readonly #actions = new Array<{ target: string; action: string; args: ReadonlyArray<unknown> }>();

    constructor(targets: ReadonlyArray<object>, options: TestRecorderOptions = {}) {
        for (const target of targets) {
            this.#observe(target as Record<string, unknown>, options);
        }
    }

    #getAllPropertyNames = (object: Record<string, unknown>) => {
        const names = new Array<string>();
        let current = object as object|undefined;
        while (current) {
            names.push(...Object.getOwnPropertyNames(current));
            current = Object.getPrototypeOf(current);
        }
        return [...new Set<string>(names)];
    };

    #observe = (object: Record<string, unknown>, { exclude = () => false }: Pick<TestRecorderOptions, 'exclude'>) => {
        const target = object.constructor.name;
        this.#objects.set(target, object);
        const actions = this.#actions;
        for (const key of this.#getAllPropertyNames(object)) {
            const value = object[key];
            if (typeof value !== 'function' || key === 'constructor')
                continue;
            if (exclude(object, key))
                continue;
            object[key] = function(...args: ReadonlyArray<unknown>) {
                actions.push({ target, action: key, args });
                return value.apply(this, args) as unknown;
            };
        }
    };

    async replayFromJSON({ actions }: ReturnType<TestRecorder['toJSON']>) {
        for (const { target, action, args } of actions) {
            console.log('Replay:', target, action, args);
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            const object = this.#objects.get(target)!;
            const result = (object[action] as (...args: ReadonlyArray<unknown>) => unknown)(...args);
            if ((result as { then?: unknown } | null | undefined)?.then)
                await result;
        }
    }

    toJSON() {
        for (const { target, action, args } of this.#actions) {
            for (const arg of args) {
                if (typeof arg === 'function')
                    throw new Error(`Cannot serialize function argument ${arg.name} of action ${target}.${action}.`);
            }
        }
        return {
            actions: this.#actions
        };
    }
}

interface MockSocketMessageEvent {
    readonly data: string;
}

type MockSocketHandlerMap = {
    open: () => void;
    message: (e: MockSocketMessageEvent) => void;
    error: () => void;
    close: () => void;
};

class MockSocketController {
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

class MockSocket {
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

const installMockSocket = (socket: MockSocket) => {
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

class TestText {
    readonly #cmView: EditorView;

    constructor(cmView: EditorView) {
        this.#cmView = cmView;
    }

    type(text: string) {
        let cursorOffset = this.#cmView.state.selection.main.anchor;
        for (const char of text) {
            const newCursorOffset = cursorOffset + 1;
            this.#cmView.dispatch(this.#cmView.state.update({
                annotations: [Transaction.userEvent.of('input.type')],
                changes: { from: cursorOffset, insert: char },
                selection: { anchor: newCursorOffset }
            }));
            cursorOffset = newCursorOffset;
        }
    }
}

class TestDomEvents {
    readonly #cmView: EditorView;

    constructor(cmView: EditorView) {
        this.#cmView = cmView;
    }

    keydown(key: string, other: Omit<KeyboardEventInit, 'key'> = {}) {
        this.#cmView
            .contentDOM
            .dispatchEvent(new KeyboardEvent('keydown', { key, ...other }));
    }

    mousemove(target: Node) {
        const event = new MouseEvent('mousemove', { bubbles: true });
        // default does not apply fake timers due to global object differences
        Object.defineProperty(event, 'timeStamp', { value: Date.now() });
        target.dispatchEvent(event);
    }
}

class TestReceiver {
    readonly #socket: MockSocketController;

    constructor(socket: MockSocketController) {
        this.#socket = socket;
    }

    changes(reason: ChangesMessage['reason'], changes: ReadonlyArray<ChangeData> = []) {
        this.#message({ type: 'changes', changes, reason });
    }

    optionsEcho(options = {}) {
        this.#message({ type: 'optionsEcho', options });
    }

    /**
     *
    readonly span: SpanData;
    readonly kinds: ReadonlyArray<string>;
    readonly sections: ReadonlyArray<InfotipSectionData>;
     */
    infotip(args: Omit<InfotipMessage, 'type'>) {
        this.#message({ type: 'infotip', ...args });
    }

    completions(
        completions: ReadonlyArray<CompletionItemData> = [],
        other: Partial<Omit<CompletionsMessage, 'completions'|'type'>> = {}
    ) {
        this.#message({ type: 'completions', completions, ...other });
    }

    completionInfo(index: number, parts: ReadonlyArray<PartData>) {
        this.#message({ type: 'completionInfo', index, parts });
    }

    #message = (message: Partial<Message<unknown, unknown>>) => {
        this.#socket.receive({ data: JSON.stringify(message) });
    };
}

export type TestDriverOptions<TExtensionServerOptions, TSlowUpdateExtensionData> = (object | { text: string; cursor?: number } | { textWithCursor: string }) & {
    keepSocketClosed?: boolean;
    options?: Partial<MirrorSharpOptions<TExtensionServerOptions, TSlowUpdateExtensionData>> & {
        initialText?: never;
        initialCursorOffset?: never;
        configureCodeMirror?: never;
    };
};

export type TestDriverTimers = {
    runOnlyPendingTimers(): void;
    advanceTimersByTime(ms: number): void;
    advanceTimersToNextTimer(): void;
    setSystemTime(now?: number | Date): void;
};

let timers: TestDriverTimers;
export const setTimers = (value: TestDriverTimers) => timers = value;

export class TestDriver<TExtensionServerOptions = never> {
    public readonly socket: MockSocketController;
    public readonly mirrorsharp: MirrorSharpInstance<TExtensionServerOptions>;
    public readonly text: TestText;
    public readonly domEvents: TestDomEvents;
    public readonly receive: TestReceiver;
    public readonly recorder: TestRecorder;

    readonly #cmView: EditorView;
    readonly #optionsForJSONOnly: TestDriverOptions<TExtensionServerOptions, unknown>;

    protected constructor(
        socket: MockSocketController,
        mirrorsharp: MirrorSharpInstance<TExtensionServerOptions>,
        optionsForJSONOnly: TestDriverOptions<TExtensionServerOptions, unknown>
    ) {
        const cmView = mirrorsharp.getCodeMirrorView();

        this.socket = socket;
        this.#cmView = cmView;
        this.mirrorsharp = mirrorsharp;
        this.text = new TestText(cmView);
        this.domEvents = new TestDomEvents(cmView);
        this.receive = new TestReceiver(socket);
        this.recorder = new TestRecorder([
            /*this.keys, */this.text, this.receive, this
        ], {
            exclude: (object, key) => object === this && (key === 'render' || key === 'toJSON')
        });

        this.#optionsForJSONOnly = optionsForJSONOnly;
    }

    getCodeMirrorView() {
        return this.#cmView;
    }

    getTextWithCursor() {
        const text = this.mirrorsharp.getText();
        const cursor = this.mirrorsharp.getCursorOffset();
        return text.slice(0, cursor) + '|' + text.slice(cursor);
    }

    setTextWithCursor(value: string) {
        const { text, cursor } = parseTextWithCursor(value);
        this.dispatchCodeMirrorTransaction({
            changes: [{
                from: 0,
                to: this.#cmView.state.doc.length,
                insert: text
            }],
            selection: { anchor: cursor }
        });
    }

    dispatchCodeMirrorTransaction(...specs: ReadonlyArray<TransactionSpec>) {
        this.#cmView.dispatch(...specs);
    }

    async completeBackgroundWork() {
        timers.runOnlyPendingTimers();
        await new Promise<void>(resolve => resolve());
        timers.runOnlyPendingTimers();
    }

    async completeBackgroundWorkAfterEach(...actions: ReadonlyArray<() => void>) {
        for (const action of actions) {
            action();
            await this.completeBackgroundWork();
        }
    }

    async advanceTimeAndCompleteNextLinting() {
        timers.advanceTimersByTime(1000);
        timers.advanceTimersToNextTimer();
        await this.completeBackgroundWork();
    }

    toJSON() {
        return {
            options: this.#optionsForJSONOnly,
            recorder: this.recorder.toJSON()
        };
    }

    static async new<TExtensionServerOptions = never, TSlowUpdateExtensionData = never>(
        options: TestDriverOptions<TExtensionServerOptions, TSlowUpdateExtensionData>
    ) {
        // eslint-disable-next-line @typescript-eslint/no-unnecessary-condition
        if (!timers)
            throw new Error('setTimers must be called before TestDriver instances can be created.');

        const initial = getInitialState(options);

        const container = document.createElement('div');
        document.body.appendChild(container);

        const socket = new MockSocket();
        installMockSocket(socket);

        const msOptions = {
            ...(options.options ?? {}),
            initialText: initial.text ?? '',
            initialCursorOffset: initial.cursor
        } as MirrorSharpOptions<TExtensionServerOptions, TSlowUpdateExtensionData>;
        const ms = mirrorsharp(container, msOptions);

        const driver = new this(socket.mock, ms, options as TestDriverOptions<TExtensionServerOptions, unknown>);

        if (options.keepSocketClosed)
            return driver;

        driver.socket.open();
        await driver.completeBackgroundWork();

        timers.runOnlyPendingTimers();
        driver.socket.sent = [];
        return driver;
    }

    static async fromJSON({ options, recorder }: ReturnType<TestDriver<unknown>['toJSON']>) {
        const driver = await this.new(options);
        await driver.recorder.replayFromJSON(recorder);
        return driver;
    }
}

function getInitialState(options: object | { text: string; cursor?: number } | { textWithCursor: string }) {
    let { text, cursor } = options as { text?: string; cursor?: number };
    if ('textWithCursor' in options)
        ({ text, cursor } = parseTextWithCursor(options.textWithCursor));
    return { text, cursor };
}

function parseTextWithCursor(value: string) {
    return {
        text: value.replace('|', ''),
        cursor: value.indexOf('|')
    };
}

export type TestDriverConstructorArguments<TExtensionServerOptions> = [
    MockSocketController,
    MirrorSharpInstance<TExtensionServerOptions>,
    TestDriverOptions<TExtensionServerOptions, unknown>
];