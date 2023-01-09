import type { EditorView } from '@codemirror/view';
import { TransactionSpec, Transaction } from '@codemirror/state';
import type {
    PartData,
    CompletionItemData,
    ChangeData,
    Message,
    ChangesMessage,
    CompletionsMessage,
    SignaturesMessage,
    InfotipMessage,
    DiagnosticData,
    UnknownMessage
} from '../interfaces/protocol';
import mirrorsharp, { MirrorSharpOptions, MirrorSharpInstance } from '../mirrorsharp';
import { installMockSocket, MockSocket, MockSocketController } from './shared/mock-socket';

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

    // cannot work in render mode -- needs adjustment
    mousemove(target: Node) {
        const event = new MouseEvent('mousemove', { bubbles: true });
        // default does not apply fake timers due to global object differences
        Object.defineProperty(event, 'timeStamp', { value: Date.now() });
        target.dispatchEvent(event);
    }

    mouseover(selector: string) {
        const target = this.#cmView.dom.querySelector(selector);
        if (!target)
            throw new Error(`Could not find element '${selector}'.`);
        target.dispatchEvent(new MouseEvent('mouseover', { bubbles: true }));
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

    signatures(message: Omit<SignaturesMessage, 'type'>) {
        this.#message({ type: 'signatures', ...message });
    }

    slowUpdate(diagnostics: ReadonlyArray<DiagnosticData>, x?: unknown) {
        this.#message({
            type: 'slowUpdate',
            diagnostics,
            x
        });
    }

    #message = (message: Partial<Exclude<Message<unknown, unknown>, UnknownMessage>>) => {
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

export class TestDriverBase<TExtensionServerOptions = never> {
    public readonly socket: MockSocketController;
    public readonly mirrorsharp: MirrorSharpInstance<TExtensionServerOptions>;
    public readonly text: TestText;
    public readonly domEvents: TestDomEvents;
    public readonly receive: TestReceiver;

    readonly #cmView: EditorView;

    protected constructor(
        socket: MockSocketController,
        mirrorsharp: MirrorSharpInstance<TExtensionServerOptions>
    ) {
        const cmView = mirrorsharp.getCodeMirrorView();

        this.socket = socket;
        this.#cmView = cmView;
        this.mirrorsharp = mirrorsharp;
        this.text = new TestText(cmView);
        this.domEvents = new TestDomEvents(cmView);
        this.receive = new TestReceiver(socket);
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

    async advanceTimeToHoverAndCompleteWork() {
        timers.advanceTimersByTime(500);
        await this.completeBackgroundWork();
    }

    async advanceTimeToSlowUpdateAndCompleteWork() {
        timers.advanceTimersByTime(1000);
        timers.advanceTimersToNextTimer();
        await this.completeBackgroundWork();
    }

    async ensureCompletionIsReadyForInteraction() {
        await this.completeBackgroundWork();
        timers.advanceTimersByTime(100);
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

        const driver = new this(socket.mock, ms);

        if (options.keepSocketClosed)
            return driver;

        driver.socket.open();
        await driver.completeBackgroundWork();

        timers.runOnlyPendingTimers();
        driver.socket.sent = [];
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
    MirrorSharpInstance<TExtensionServerOptions>
];