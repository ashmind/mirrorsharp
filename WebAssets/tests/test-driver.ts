import type { EditorView } from '@codemirror/next/view';
import { EditorSelection } from '@codemirror/next/state';
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

class TestKeys {
    private readonly editable: HTMLElement;
    private readonly getCursor: () => number;

    constructor(editable: HTMLElement, getCursor: () => number) {
        this.editable = editable;
        this.getCursor = getCursor;
    }

    type(text: string) {
        const editable = this.editable;
        editable.focus();

        const htmlOffset = this.mapToHTMLOffset(this.getCursor());
        editable.innerHTML = spliceString(editable.innerHTML, htmlOffset, 0, text);
        keyboard.dispatchEventsForInput(text, editable);
    }

    backspace(count: number) {
        const editable = this.editable;
        for (let i = 0; i < count; i++) {
            const htmlOffset = this.mapToHTMLOffset(this.getCursor() - 1);
            editable.innerHTML = spliceString(editable.innerHTML, htmlOffset, 1);
            keyboard.dispatchEventsForAction('backspace', editable);
        }
    }

    press(keys: string) {
        keyboard.dispatchEventsForAction(keys, this.editable);
    }

    private mapToHTMLOffset(textOffset: number) {
        let htmlOffset = 0;

        const parts = this.editable.innerHTML.split(/(<[^>]+>)/);
        let textLengthBefore = 0;
        for (const part of parts) {
            const isHTML = part.startsWith('<');
            if (isHTML) {
                htmlOffset += part.length;
                continue;
            }

            if (textLengthBefore + part.length > textOffset) {
                htmlOffset += textOffset - textLengthBefore;
                break;
            }

            htmlOffset += part.length;
            textLengthBefore += part.length;
        }

        return htmlOffset;
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
        options: ({}|{ text: string; cursor?: number }|{ textWithCursor: string })&{ options?: Partial<MirrorSharpOptions<TExtensionServerOptions, TSlowUpdateExtensionData>> }
    ) {
        const initial = getInitialState(options);

        const initialTextarea = document.createElement('textarea');
        initialTextarea.value = initial.text ?? '';
        document.body.appendChild(initialTextarea);

        const socket = new MockSocket();
        global.WebSocket = function() { return socket; };

        const ms = mirrorsharp(initialTextarea, (options.options ?? {}) as MirrorSharpOptions<TExtensionServerOptions, TSlowUpdateExtensionData>);

        delete global.WebSocket;

        const cmView = ms.getCodeMirrorView();

        if (initial.cursor) {
            cmView.dispatch(
                cmView.state.t().setSelection(EditorSelection.single(initial.cursor))
            );
        }

        const input = cmView.dom;

        const driver = new TestDriver(
            socket, ms, cmView,
            new TestKeys(input, () => cmView.state.selection.primaryIndex),
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