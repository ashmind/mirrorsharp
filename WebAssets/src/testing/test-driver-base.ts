import type { TransactionSpec } from '@codemirror/state';
import type { EditorView } from '@codemirror/view';
import mirrorsharp, { MirrorSharpOptions, MirrorSharpInstance } from '../mirrorsharp';
import { installMockSocket, MockSocket, MockSocketController } from './shared/mock-socket';
import { TestReceiver } from './shared/test-receiver';

export type TestDriverOptions<TExtensionServerOptions, TSlowUpdateExtensionData> = (object | { text: string; cursorOffset?: number } | { textWithCursor: string }) & {
    skipSocketOpen?: boolean;
} & Omit<Partial<MirrorSharpOptions<TExtensionServerOptions, TSlowUpdateExtensionData>>, 'text' | 'cursorOffset'>;

export type TestDriverTimers = {
    runOnlyPendingTimers(): void;
    advanceTimersByTime(ms: number): void;
    advanceTimersToNextTimer(): void;
    setSystemTime(now?: number | Date): void;
};

let timers: TestDriverTimers;
export const setTimers = (value: TestDriverTimers) => timers = value;

export class TestDriverBase<TExtensionServerOptions = void, TSlowUpdateExtensionData = void> {
    public readonly socket: MockSocketController;
    public readonly mirrorsharp: MirrorSharpInstance<TExtensionServerOptions>;
    public readonly receive: TestReceiver<TExtensionServerOptions, TSlowUpdateExtensionData>;

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
        const { text, cursorOffset } = parseTextWithCursor(value);
        this.dispatchCodeMirrorTransaction({
            changes: [{
                from: 0,
                to: this.#cmView.state.doc.length,
                insert: text
            }],
            selection: { anchor: cursorOffset }
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

    static async new<TExtensionServerOptions = void, TSlowUpdateExtensionData = void>(
        options: TestDriverOptions<TExtensionServerOptions, TSlowUpdateExtensionData>
    ) {
        // eslint-disable-next-line @typescript-eslint/no-unnecessary-condition
        if (!timers)
            throw new Error('setTimers must be called before TestDriver instances can be created.');

        const { skipSocketOpen, ...mirrorsharpOptions } = normalizeOptions(options);

        const container = document.createElement('div');
        document.body.appendChild(container);

        const socket = this.newMockSocket();
        installMockSocket(socket, { manualOpen: skipSocketOpen });

        const ms = mirrorsharp<TExtensionServerOptions, TSlowUpdateExtensionData>(container, {
            ...mirrorsharpOptions,
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            serviceUrl: null!
        });

        const driver = new this<TExtensionServerOptions, TSlowUpdateExtensionData>(socket.mock, ms);

        if (skipSocketOpen)
            return driver;

        await driver.completeBackgroundWork();

        timers.runOnlyPendingTimers();
        driver.socket.sent = [];
        return driver;
    }
}

const normalizeOptions = <O, U>(options: TestDriverOptions<O, U>) => {
    if ('textWithCursor' in options) {
        const { textWithCursor, ...rest } = options;
        const { text, cursorOffset } = parseTextWithCursor(textWithCursor);
        return { ...rest, text, cursorOffset };
    }

    return options;
};

const parseTextWithCursor = (value: string) => ({
    text: value.replace('|', ''),
    cursorOffset: value.indexOf('|')
});

export type TestDriverConstructorArguments<TExtensionServerOptions> = [
    MockSocketController,
    MirrorSharpInstance<TExtensionServerOptions>
];