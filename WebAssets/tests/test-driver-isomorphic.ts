import type { EditorView } from '@codemirror/view';
import { TransactionSpec, Transaction } from '@codemirror/state';
import type { PartData, CompletionItemData, ChangeData, ChangesMessage, CompletionsMessage, Message, SpanData, InfotipMessage } from '../ts/interfaces/protocol';
import mirrorsharp, { MirrorSharpOptions, MirrorSharpInstance } from '../ts/mirrorsharp';

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
            const object = this.#objects.get(target)!;
            const result = (object[action] as (...args: ReadonlyArray<unknown>) => unknown)(...args);
            if ((result as { then?: unknown })?.then)
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

class MockSocket {
    public createdCount = 0;
    public sent: Array<string>;

    readonly #handlers = {} as {
        open?: Array<() => void>;
        message?: Array<(e: MockSocketMessageEvent) => void>;
        close?: Array<() => void>;
    };

    constructor() {
        this.sent = [];
    }

    send(message: string) {
        this.sent.push(message);
    }

    trigger(event: 'open'): void;
    trigger(event: 'message', e: MockSocketMessageEvent): void;
    trigger(event: 'close'): void;
    trigger(...[event, e]: ['open']|['message', MockSocketMessageEvent]|['close']) {
        // https://github.com/microsoft/TypeScript/issues/37505 ?
        for (const handler of (this.#handlers[event] ?? [])) {
            // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-call
            (handler as (...args: any) => void)(e);
        }
    }

    addEventListener(event: 'open', handler: () => void): void;
    addEventListener(event: 'message', handler: (e: MockSocketMessageEvent) => void): void;
    addEventListener(event: 'close', handler: () => void): void;
    addEventListener(...[event, handler]: ['open', () => void]|['message', (e: MockSocketMessageEvent) => void]|['close', () => void]) {
        let handlers = this.#handlers[event];
        if (!handlers) {
            handlers = [] as NonNullable<typeof handlers>;
            this.#handlers[event] = handlers;
        }
        handlers.push(handler);
    }
}

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
                annotations: [Transaction.userEvent.of('input')],
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
    readonly #socket: MockSocket;

    constructor(socket: MockSocket) {
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
        this.#socket.trigger('message', { data: JSON.stringify(message) });
    };
}

export type TestDriverOptions<TExtensionServerOptions, TSlowUpdateExtensionData> = ({}|{ text: string; cursor?: number }|{ textWithCursor: string }) & {
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
    public readonly socket: MockSocket;
    public readonly mirrorsharp: MirrorSharpInstance<TExtensionServerOptions>;
    public readonly text: TestText;
    public readonly domEvents: TestDomEvents;
    public readonly receive: TestReceiver;
    public readonly recorder: TestRecorder;

    readonly #cmView: EditorView;
    readonly #optionsForJSONOnly: TestDriverOptions<TExtensionServerOptions, unknown>;

    protected constructor(
        socket: MockSocket,
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

        if (globalThis.WebSocket instanceof MockSocket)
            throw new Error(`Global WebSocket is already set up in this context.`);

        const socket = new MockSocket();
        (globalThis as unknown as { WebSocket: () => Partial<WebSocket> }).WebSocket = function() {
            socket.createdCount += 1;
            return socket;
        };

        const msOptions = {
            ...(options.options ?? {}),
            initialText: initial.text ?? '',
            initialCursorOffset: initial.cursor
        } as MirrorSharpOptions<TExtensionServerOptions, TSlowUpdateExtensionData>;
        const ms = mirrorsharp(container, msOptions);

        const driver = new this(socket, ms, options as TestDriverOptions<TExtensionServerOptions, unknown>);

        if (options.keepSocketClosed)
            return driver;

        driver.socket.trigger('open');
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

function getInitialState(options: {}|{ text: string; cursor?: number }|{ textWithCursor: string }) {
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
    MockSocket,
    MirrorSharpInstance<TExtensionServerOptions>,
    TestDriverOptions<TExtensionServerOptions, unknown>
];