import { ensureDefined } from '../helpers/ensure-defined';
import type { Message, ServerOptions } from './messages';

const stateCommandMap = { cancel: 'X', force: 'F' } as Readonly<{
    cancel: 'X';
    force: 'F';
    [key: number]: undefined;
}>;

export type ReplaceTextCommand = {
    start: number,
    length: number,
    newText: string,
    cursorIndexAfter: number,
    reason?: string | null
};

type HandlerMap<O, U> = {
    open:    () => void,
    message: (data: Message<O, U>) => void,
    close:   () => void
};

// Defaults are 'unknown' rather than 'void', as it exists for internal convenience,
// and we assume in most cases this is not 'void'. Anything public should have 'void' though.
export class Connection<TExtensionServerOptions = unknown, TSlowUpdateExtensionData = unknown> {
    readonly #url: string;

    readonly #listeners = {
        open:    [],
        message: [],
        close:   []
    } as {
        [K in keyof HandlerMap<TExtensionServerOptions, TSlowUpdateExtensionData>]:
            Array<HandlerMap<TExtensionServerOptions, TSlowUpdateExtensionData>[K]>
    };

    #socket: WebSocket | undefined;
    #openPromise!: Promise<void>;
    #resolveDelayedOpenPromise: (() => void) | undefined | null;

    #mustBeClosed = false;
    #reopenPeriod = 0;
    #reopenPeriodResetTimer: ReturnType<typeof setTimeout> | undefined | null;
    #reopening = false;

    #removeInternalListeners: () => void;

    constructor(url: string, { closed }: { closed: boolean | undefined }) {
        this.#url = url;

        if (!closed) {
            this.open();
        }
        else {
            this.#openPromise = new Promise(resolve => {
                this.#resolveDelayedOpenPromise = resolve;
            });
        }

        this.#removeInternalListeners = this.addEventListeners({
            close: () => this.#tryToReopen()
        });
    }

    addEventListeners(listeners: Partial<HandlerMap<TExtensionServerOptions, TSlowUpdateExtensionData>>) {
        const removeEach = [] as Array<() => void>;
        for (const key in listeners) {
            const list = this.#listeners[key as keyof typeof listeners];
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            const listener = listeners[key as keyof typeof listeners]!;

            list.push(
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                listener as any
            );
            removeEach.push(() => {
                // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-call
                const index = list.indexOf(listener as any);
                if (index >= 0)
                    list.splice(index, 1);
            });
        }

        return () => {
            for (const remove of removeEach) {
                remove();
            }
        };
    }

    open() {
        this.#socket = new WebSocket(this.#url);
        this.#openPromise = new Promise(resolve => {
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            this.#socket!.addEventListener('open', () => {
                this.#reopenPeriodResetTimer = setTimeout(() => { this.#reopenPeriod = 0; }, this.#reopenPeriod);
                if (this.#resolveDelayedOpenPromise) {
                    this.#resolveDelayedOpenPromise();
                    this.#resolveDelayedOpenPromise = null;
                }
                resolve();
            });
        });
        this.#addEventListenersToSocket();
    }

    #addEventListenersToSocket() {
        if (!this.#socket)
            throw new Error('Internal error: Socket was not created before adding listeners.');

        this.#socket.addEventListener('open', () => {
            for (const listener of this.#listeners['open']) {
                listener();
            }
        });
        this.#socket.addEventListener('message', e => {
            const data = JSON.parse(e.data as string) as Message<TExtensionServerOptions, TSlowUpdateExtensionData>;
            for (const listener of this.#listeners['message']) {
                listener(data);
            }
        });
        this.#socket.addEventListener('close', () => {
            for (const listener of this.#listeners['close']) {
                listener();
            }
        });
    }

    #tryToReopen() {
        if (this.#mustBeClosed || this.#reopening)
            return;

        if (this.#reopenPeriodResetTimer) {
            clearTimeout(this.#reopenPeriodResetTimer);
            this.#reopenPeriodResetTimer = null;
        }

        this.#reopening = true;
        setTimeout(() => {
            this.open();
            this.#reopening = false;
        }, this.#reopenPeriod);
        if (this.#reopenPeriod < 60000)
            this.#reopenPeriod = Math.min(5 * (this.#reopenPeriod + 200), 60000);
    }

    async #sendIfOpen(command: string) {
        if (this.#mustBeClosed)
            throw `Cannot send command '${command}' after the close() call.`;

        if (!this.isOpen()) {
            // eslint-disable-next-line @typescript-eslint/restrict-template-expressions
            console.warn(`Dropped command '${command}' because the socket state is ${this.#socket?.readyState}.`);
            return;
        }

        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        this.#socket!.send(command);
    }

    isOpen() {
        return this.#socket?.readyState === WebSocket.OPEN;
    }

    sendReplaceText({ start, length, cursorIndexAfter, newText, reason }: ReplaceTextCommand) {
        return this.#sendIfOpen('R' + start + ':' + length + ':' + cursorIndexAfter + ':' + (reason ?? '') + ':' + newText);
    }

    sendMoveCursor(cursorIndex: number) {
        return this.#sendIfOpen('M' + cursorIndex);
    }

    sendTypeChar(char: string) {
        return this.#sendIfOpen('C' + char);
    }

    sendCompletionState(indexOrCommand: 'info'|'force'|number|'cancel', indexIfInfo?: number) {
        // common bug -- the specific flow is not fully clear yet,
        // but there is no reason to send null/undefined to server if it will fail anyway
        ensureDefined(indexOrCommand, 'completion command');
        const argument = indexOrCommand !== 'info'
            ? (stateCommandMap[indexOrCommand] ?? indexOrCommand)
            : 'I' + ensureDefined(indexIfInfo, 'completion info index');
        return this.#sendIfOpen('S' + argument);
    }

    sendSignatureHelpState(command: 'force'|'cancel') {
        return this.#sendIfOpen('P' + stateCommandMap[command]);
    }

    sendRequestInfoTip(cursorIndex: number) {
        return this.#sendIfOpen('I' + cursorIndex);
    }

    sendSlowUpdate() {
        return this.#sendIfOpen('U');
    }

    sendApplyDiagnosticAction(actionId: number) {
        return this.#sendIfOpen('F' + actionId);
    }

    sendSetOptions(options: Partial<ServerOptions> & Partial<TExtensionServerOptions>) {
        const optionPairs = [];
        for (const key in options) {
            optionPairs.push(key + '=' + (options as Record<string, string>)[key]);
        }
        return this.#sendIfOpen('O' + optionPairs.join(','));
    }

    sendRequestSelfDebugData() {
        return this.#sendIfOpen('Y');
    }

    close() {
        this.#mustBeClosed = true;
        this.#removeInternalListeners();
        this.#socket?.close();
    }
}