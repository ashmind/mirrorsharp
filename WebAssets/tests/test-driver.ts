import type { PartData, CompletionItemData, ChangeData } from '../ts/interfaces/protocol';
import mirrorsharp, { MirrorSharpOptions, MirrorSharp } from '../ts/mirrorsharp';
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

type MockSocketEventMap = {
    'open': undefined;
    'message': { readonly data: string };
};

class MockSocket {
    public sent: Array<string>;
    private readonly handlers: {
        [K in keyof MockSocketEventMap]?: Array<(e: MockSocketEventMap[K]) => void>
    };

    constructor() {
        this.sent = [];
        this.handlers = {};
    }

    send(message: string) {
        this.sent.push(message);
    }

    trigger<K extends keyof MockSocketEventMap>(event: K, e?: MockSocketEventMap[K]) {
        // https://github.com/microsoft/TypeScript/issues/37505 ?
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        for (const handler of (this.handlers[event] as any || [])) {
            handler(e);
        }
    }

    addEventListener<K extends keyof MockSocketEventMap>(event: K, handler: (e: MockSocketEventMap[K]) => void) {
        // https://github.com/microsoft/TypeScript/issues/37505 ?
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        (this.handlers[event] = this.handlers[event] as any || []).push(handler);
    }
}

class MockTextRange {
    // eslint-disable-next-line @typescript-eslint/no-empty-function
    getBoundingClientRect() {}
    getClientRects(): [] { return []; }
}
global.document.body.createTextRange = () => new MockTextRange();

class TestKeys {
    private input: HTMLTextAreaElement|HTMLInputElement;
    private getCursor: () => number;

    constructor(input: HTMLTextAreaElement|HTMLInputElement, getCursor: () => number) {
        this.input = input;
        this.getCursor = getCursor;
    }

    type(text: string) {
        const input = this.input;
        input.focus();
        input.value = spliceString(input.value, this.getCursor(), 0, text);
        keyboard.dispatchEventsForInput(text, input);
    }

    backspace(count: number) {
        const input = this.input;
        for (let i = 0; i < count; i++) {
            input.value = spliceString(input.value, this.getCursor() - 1, 1);
            keyboard.dispatchEventsForAction('backspace', this.input);
        }
    }

    press(keys: string) {
        keyboard.dispatchEventsForAction(keys, this.input);
    }
}

class TestReceiver {
    private socket: MockSocket;

    constructor(socket: MockSocket) {
        this.socket = socket;
    }

    changes(changes: ReadonlyArray<ChangeData> = [], reason = '') {
        this.socket.trigger('message', { data: JSON.stringify({type: 'changes', changes, reason}) });
    }

    optionsEcho(options = {}) {
        this.socket.trigger('message', { data: JSON.stringify({type: 'optionsEcho', options}) });
    }

    completions(completions: ReadonlyArray<CompletionItemData> = [], { span = {}, commitChars = null, suggestion = null } = {}) {
        this.socket.trigger('message', { data: JSON.stringify({type: 'completions', completions, span, commitChars, suggestion}) });
    }

    completionInfo(index: number, parts: ReadonlyArray<PartData>) {
        this.socket.trigger('message', { data: JSON.stringify({type: 'completionInfo', index, parts}) });
    }
}

class TestDriver<TExtensionData = never> {
    public readonly socket: MockSocket;
    public readonly mirrorsharp: MirrorSharp<TExtensionData>;
    public readonly keys: TestKeys;
    public readonly receive: TestReceiver;

    private readonly cm: CodeMirror.Editor;

    private constructor(
        socket: MockSocket,
        mirrorsharp: MirrorSharp<TExtensionData>,
        cm: CodeMirror.Editor,
        keys: TestKeys,
        receive: TestReceiver
    ) {
        this.socket = socket;
        this.mirrorsharp = mirrorsharp;
        this.cm = cm;
        this.keys = keys;
        this.receive = receive;
    }

    getCodeMirror() {
        return this.cm;
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

    static async new<TExtensionData>(options: ({}|{ text: string; cursor?: number }|{ textWithCursor: string })&{ options?: Partial<MirrorSharpOptions<TExtensionData>> }) {
        const initial = getInitialState(options);

        const initialTextarea = document.createElement('textarea');
        initialTextarea.value = initial.text || '';
        document.body.appendChild(initialTextarea);

        const socket = new MockSocket();
        global.WebSocket = function() { return socket; };

        const ms = mirrorsharp(initialTextarea, (options.options || {}) as MirrorSharpOptions<TExtensionData>);
        const cm = ms.getCodeMirror();

        delete global.WebSocket;

        if (initial.cursor)
            cm.setCursor(cm.posFromIndex(initial.cursor));

        const input = cm.getWrapperElement().querySelector('textarea')!;

        const driver = new TestDriver(
            socket, ms, cm,
            new TestKeys(input, () => cm.indexFromPos(cm.getCursor())),
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