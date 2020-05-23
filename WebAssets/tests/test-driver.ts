import type { EditorView } from '@codemirror/next/view';
import type { Transaction, TransactionSpec } from '@codemirror/next/state';
import { advanceBy as advanceDateBy } from 'jest-date-mock';
import type { PartData, CompletionItemData, ChangeData } from '../ts/interfaces/protocol';
import mirrorsharp, { MirrorSharpOptions, MirrorSharpInstance } from '../ts/mirrorsharp';
import { Keyboard } from 'keysim';

jest.useFakeTimers();
(() => {
    // clean JSDOM between tests
    const emptyHTML = document.body.innerHTML;
    afterEach(() => document.body.innerHTML = emptyHTML);
})();

const keyboard = Keyboard.US_ENGLISH;

const spliceString = (string: string, start: number, length: number, newString = '') =>
    string.substring(0, start) + newString + string.substring(start + length);

interface MockSocketMessageEvent {
    readonly data: string;
}

class MockSocket {
    public createdCount = 0;
    public sent: Array<string>;
    private readonly handlers: {
        open?: Array<() => void>;
        message?: Array<(e: MockSocketMessageEvent) => void>;
        close?: Array<() => void>;
    };

    constructor() {
        this.sent = [];
        this.handlers = {};
    }

    send(message: string) {
        this.sent.push(message);
    }

    trigger(event: 'open'): void;
    trigger(event: 'message', e: MockSocketMessageEvent): void;
    trigger(event: 'close'): void;
    trigger(...[event, e]: ['open']|['message', MockSocketMessageEvent]|['close']) {
        // https://github.com/microsoft/TypeScript/issues/37505 ?
        for (const handler of (this.handlers[event] ?? [])) {
            // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-call
            (handler as (...args: any) => void)(e);
        }
    }

    addEventListener(event: 'open', handler: () => void): void;
    addEventListener(event: 'message', handler: (e: MockSocketMessageEvent) => void): void;
    addEventListener(event: 'close', handler: () => void): void;
    addEventListener(...[event, handler]: ['open', () => void]|['message', (e: MockSocketMessageEvent) => void]|['close', () => void]) {
        let handlers = this.handlers[event];
        if (!handlers) {
            handlers = [] as NonNullable<typeof handlers>;
            this.handlers[event] = handlers;
        }
        handlers.push(handler);
    }
}

class MockTextRange {
    // eslint-disable-next-line @typescript-eslint/no-empty-function
    getBoundingClientRect() {}
    getClientRects(): [] { return []; }
}

class MockSelection {
    readonly anchorNode = null;
    readonly anchorOffset = 0;
    readonly focusNode = null;
    readonly focusOffset = 0;
}

global.document.body.createTextRange = () => new MockTextRange();
global.document.getSelection = () => new MockSelection();

(HTMLElement.prototype as unknown as { elementFromPoint: () => HTMLElement|null }).elementFromPoint = () => { throw 'hmm!!!'; };

class TestKeys {
    readonly #cmView: EditorView;

    constructor(cmView: EditorView) {
        this.#cmView = cmView;
    }

    backspace(count: number) {
        const { node, offset } = this.getCursorInfo();
        for (let i = 0; i < count; i++) {
            node.textContent = spliceString(node.textContent!, offset, 1);
            keyboard.dispatchEventsForAction('backspace', this.#cmView.contentDOM);
        }
    }

    press(keys: string) {
        keyboard.dispatchEventsForAction(keys, this.#cmView.contentDOM);
    }

    private getCursorInfo() {
        const index = this.#cmView.state.selection.primary.from;
        return this.#cmView.domAtPos(index);
    }
}

class TestText {
    readonly #cmView: EditorView;

    constructor(cmView: EditorView) {
        this.#cmView = cmView;
    }

    type(text: string) {
        let cursorOffset = this.#cmView.state.selection.primary.anchor;
        for (const char of text) {
            const newCursorOffset = cursorOffset + 1;
            this.#cmView.dispatch(this.#cmView.state.update({
                changes: { from: cursorOffset, insert: char },
                selection: { anchor: newCursorOffset }
            }));
            cursorOffset = newCursorOffset;
        }
    }
}

class TestReceiver {
    private readonly socket: MockSocket;

    constructor(socket: MockSocket) {
        this.socket = socket;
    }

    changes(changes: ReadonlyArray<ChangeData> = [], reason = '') {
        this.socket.trigger('message', { data: JSON.stringify({ type: 'changes', changes, reason }) });
    }

    optionsEcho(options = {}) {
        this.socket.trigger('message', { data: JSON.stringify({ type: 'optionsEcho', options }) });
    }

    completions(completions: ReadonlyArray<CompletionItemData> = [], { span = {}, commitChars = null, suggestion = null } = {}) {
        this.socket.trigger('message', { data: JSON.stringify({ type: 'completions', completions, span, commitChars, suggestion }) });
    }

    completionInfo(index: number, parts: ReadonlyArray<PartData>) {
        this.socket.trigger('message', { data: JSON.stringify({ type: 'completionInfo', index, parts }) });
    }
}

class TestDriver<TExtensionServerOptions = never> {
    public readonly socket: MockSocket;
    public readonly mirrorsharp: MirrorSharpInstance<TExtensionServerOptions>;
    public readonly keys: TestKeys;
    public readonly text: TestText;
    public readonly receive: TestReceiver;

    readonly #cmView: EditorView;

    private constructor(socket: MockSocket, mirrorsharp: MirrorSharpInstance<TExtensionServerOptions>) {
        const cmView = mirrorsharp.getCodeMirrorView();

        this.socket = socket;
        this.#cmView = cmView;
        this.mirrorsharp = mirrorsharp;
        this.keys = new TestKeys(cmView);
        this.text = new TestText(cmView);
        this.receive = new TestReceiver(socket);
    }

    getCodeMirrorView() {
        return this.#cmView;
    }

    dispatchCodeMirrorTransaction(...specs: ReadonlyArray<TransactionSpec>) {
        this.#cmView.dispatch(this.#cmView.state.update(...specs));
    }

    async completeBackgroundWork() {
        jest.runOnlyPendingTimers();
        await new Promise(resolve => resolve());
        jest.runOnlyPendingTimers();
    }

    async completeBackgroundWorkAfterEach(...actions: ReadonlyArray<() => void>) {
        for (const action of actions) {
            action();
            await this.completeBackgroundWork();
        }
    }

    async advanceTimeAndCompleteNextLinting() {
        advanceDateBy(1000);
        jest.advanceTimersToNextTimer();
        await this.completeBackgroundWork();
    }

    static async new<TExtensionServerOptions = never, TSlowUpdateExtensionData = never>(
        options: ({}|{ text: string; cursor?: number }|{ textWithCursor: string }) & {
            keepSocketClosed?: boolean;
            options?: Partial<MirrorSharpOptions<TExtensionServerOptions, TSlowUpdateExtensionData>> & {
                initialText?: never;
                initialCursorOffset?: never;
                configureCodeMirror?: never;
            };
        }
    ) {
        const initial = getInitialState(options);

        const container = document.createElement('div');
        document.body.appendChild(container);

        if (global.WebSocket instanceof MockSocket)
            throw new Error(`Global WebSocket is already set up in this context.`);

        const socket = new MockSocket();
        global.WebSocket = function() {
            socket.createdCount += 1;
            return socket;
        };

        const msOptions = {
            ...(options.options ?? {}),
            initialText: initial.text ?? '',
            initialCursorOffset: initial.cursor
        } as MirrorSharpOptions<TExtensionServerOptions, TSlowUpdateExtensionData>;
        const ms = mirrorsharp(container, msOptions);

        const driver = new TestDriver(socket, ms);

        if (options.keepSocketClosed)
            return driver;

        driver.socket.trigger('open');
        await driver.completeBackgroundWork();

        jest.runOnlyPendingTimers();
        driver.socket.sent = [];
        return driver;
    }
}

function getInitialState(options: {}|{ text: string; cursor?: number }|{ textWithCursor: string }) {
    let { text, cursor } = options as { text?: string; cursor?: number };
    if ('textWithCursor' in options) {
        text = options.textWithCursor.replace('|', '');
        cursor = options.textWithCursor.indexOf('|');
    }
    return { text, cursor };
}


let savedWebSocket: (typeof global.WebSocket)|undefined;
beforeEach(() => {
    savedWebSocket = global.WebSocket;
});

afterEach(() => {
    global.WebSocket = savedWebSocket!;
});

export { TestDriver };