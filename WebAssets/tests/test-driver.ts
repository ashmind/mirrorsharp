import type { EditorView } from '@codemirror/next/view';
import type { Transaction } from '@codemirror/next/state';
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
    public sent: Array<string>;
    private readonly handlers: {
        open?: Array<() => void>;
        message?: Array<(e: MockSocketMessageEvent) => void>;
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
    trigger(...[event, e]: ['open']|['message', MockSocketMessageEvent]) {
        // https://github.com/microsoft/TypeScript/issues/37505 ?
        for (const handler of (this.handlers[event] ?? [])) {
            // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-call
            (handler as (...args: any) => void)(e);
        }
    }

    addEventListener(event: 'open', handler: () => void): void;
    addEventListener(event: 'message', handler: (e: MockSocketMessageEvent) => void): void;
    addEventListener(...[event, handler]: ['open', () => void]|['message', (e: MockSocketMessageEvent) => void]) {
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
    private readonly cmView: EditorView;

    constructor(cmView: EditorView) {
        this.cmView = cmView;
    }

    type(text: string) {
        this.cmView.contentDOM.focus();

        const { node, offset } = this.getCursorInfo();

        node.textContent = spliceString(node.textContent ?? '', offset, 0, text);
        keyboard.dispatchEventsForInput(text, this.cmView.contentDOM);
    }

    backspace(count: number) {
        const { node, offset } = this.getCursorInfo();
        for (let i = 0; i < count; i++) {
            node.textContent = spliceString(node.textContent!, offset, 1);
            keyboard.dispatchEventsForAction('backspace', this.cmView.contentDOM);
        }
    }

    press(keys: string) {
        keyboard.dispatchEventsForAction(keys, this.cmView.contentDOM);
    }

    private getCursorInfo() {
        const index = this.cmView.state.selection.primary.from;
        return this.cmView.domAtPos(index);
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
    public readonly receive: TestReceiver;

    private readonly cmView: EditorView;

    private constructor(
        socket: MockSocket,
        mirrorsharp: MirrorSharpInstance<TExtensionServerOptions>,
        cmView: EditorView,
        keys: TestKeys,
        receive: TestReceiver
    ) {
        this.socket = socket;
        this.mirrorsharp = mirrorsharp;
        this.cmView = cmView;
        this.keys = keys;
        this.receive = receive;
    }

    getCodeMirrorView() {
        return this.cmView;
    }

    dispatchCodeMirrorTransaction(setup: (t: Transaction) => Transaction) {
        this.cmView.dispatch(setup(this.cmView.state.t()));
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

    static async new<TExtensionServerOptions = never, TSlowUpdateExtensionData = never>(
        options: ({}|{ text: string; cursor?: number }|{ textWithCursor: string }) & {
            options?: Partial<MirrorSharpOptions<TExtensionServerOptions, TSlowUpdateExtensionData>> & { configureCodeMirror?: never };
        }
    ) {
        const initial = getInitialState(options);

        const container = document.createElement('div');
        document.body.appendChild(container);

        const socket = new MockSocket();
        global.WebSocket = function() { return socket; };

        const msOptions = {
            ...(options.options ?? {}),
            initialText: initial.text ?? '',
            initialCursorOffset: initial.cursor
        } as MirrorSharpOptions<TExtensionServerOptions, TSlowUpdateExtensionData>;
        const ms = mirrorsharp(container, msOptions);

        delete global.WebSocket;

        const cmView = ms.getCodeMirrorView();

        const driver = new TestDriver(
            socket, ms, cmView,
            new TestKeys(cmView),
            new TestReceiver(socket)
        );

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

export { TestDriver };