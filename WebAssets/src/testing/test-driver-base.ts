import type { TransactionSpec } from '@codemirror/state';
import type { EditorView } from '@codemirror/view';
import mirrorsharp, { MirrorSharpOptions, MirrorSharpInstance } from '../mirrorsharp';
import { installMockSocket, MockSocket, MockSocketController } from './shared/mock-socket';
import { TestReceiver } from './shared/test-receiver';

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

    protected static newMockSocket(): MockSocket {
        return new MockSocket();
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

        const socket = this.newMockSocket();
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