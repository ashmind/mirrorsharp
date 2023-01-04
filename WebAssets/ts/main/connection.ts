import type { Message, ServerOptions } from '../interfaces/protocol';
// import type { SelfDebug } from './self-debug';
import { addEvents } from '../helpers/add-events';
import { ensureDefined } from '../helpers/ensure-defined';

const eventKeys = ['open', 'message', 'error', 'close'] as const;

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

export type ConnectionEventMap<TExtensionServerOptions, TSlowUpdateExtensionData> = {
    open: (e: Event) => void;
    message: (data: Message<TExtensionServerOptions, TSlowUpdateExtensionData>, e: MessageEvent) => void;
    error: (e: ErrorEvent) => void;
    close: (e: CloseEvent) => void;
};

export class Connection<TExtensionServerOptions, TSlowUpdateExtensionData = unknown> {
    readonly #url: string;
    // readonly #selfDebug: SelfDebug|null;

    readonly #handlers = {
        open:    [] as Array<ConnectionEventMap<TExtensionServerOptions, TSlowUpdateExtensionData>['open']>,
        message: [] as Array<ConnectionEventMap<TExtensionServerOptions, TSlowUpdateExtensionData>['message']>,
        error:   [] as Array<ConnectionEventMap<TExtensionServerOptions, TSlowUpdateExtensionData>['error']>,
        close:   [] as Array<ConnectionEventMap<TExtensionServerOptions, TSlowUpdateExtensionData>['close']>
    } as const;

    #socket: WebSocket | undefined;
    #openPromise!: Promise<void>;
    #resolveDelayedOpenPromise: (() => void) | undefined | null;

    #mustBeClosed = false;
    #reopenPeriod = 0;
    #reopenPeriodResetTimer: ReturnType<typeof setTimeout> | undefined | null;
    #reopening = false;

    #removeEvents: () => void;

    constructor(url: string, /* selfDebug: SelfDebug|null, */ { delayedOpen }: { delayedOpen?: boolean } = {}) {
        this.#url = url;
        // this.#selfDebug = selfDebug;

        if (!delayedOpen) {
            this.open();
        }
        else {
            this.#openPromise = new Promise(resolve => {
                this.#resolveDelayedOpenPromise = resolve;
            });
        }

        this.#removeEvents = addEvents(this, {
            error: this.#tryToReopen,
            close: this.#tryToReopen
        });
    }

    open = () => {
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

        for (const key of eventKeys) {
            const handlersByKey = this.#handlers[key];
            this.#socket.addEventListener(key, e => {
                const handlerArguments = [e] as [CloseEvent|Event]|[Message<unknown, unknown>, MessageEvent];
                if (key === 'message') {
                    const data = JSON.parse((e as MessageEvent).data as string) as Message<unknown, unknown>;
                    if (data.type === 'self:debug') {
                        for (const entry of data.log) {
                            (entry as { time: Date }).time = new Date(entry.time as unknown as string);
                        }
                    }
                    // if (this.#selfDebug)
                    //     this.#selfDebug.log('before', JSON.stringify(data));
                    (handlerArguments as [Message<unknown, unknown>, MessageEvent]).unshift(data);
                }
                for (const handler of handlersByKey) {
                    // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-call
                    (handler as any)(...handlerArguments);
                }
                // if (this.#selfDebug && key === 'message')
                //     this.#selfDebug.log('after', JSON.stringify(handlerArguments[0]));
            });
        }
    };

    #tryToReopen = () => {
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
    };

    #sendIfOpen = async (command: string) => {
        if (this.#mustBeClosed)
            throw `Cannot send command '${command}' after the close() call.`;

        if (!this.isOpen()) {
            // eslint-disable-next-line @typescript-eslint/restrict-template-expressions
            console.warn(`Dropped command '${command}' because the socket state is ${this.#socket?.readyState}.`);
            return;
        }

        // if (this.#selfDebug)
        //     this.#selfDebug.log('send', command);
        this.#socket.send(command);
    };

    on<TKey extends keyof ConnectionEventMap<TExtensionServerOptions, TSlowUpdateExtensionData>>(
        key: TKey,
        handler: ConnectionEventMap<TExtensionServerOptions, TSlowUpdateExtensionData>[TKey]
    ) {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-call
        this.#handlers[key].push(handler as any);
    }

    off<TKey extends keyof ConnectionEventMap<TExtensionServerOptions, TSlowUpdateExtensionData>>(
        key: TKey,
        handler: ConnectionEventMap<TExtensionServerOptions, TSlowUpdateExtensionData>[TKey]
    ) {
        const list = this.#handlers[key];
        // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-call
        const index = list.indexOf(handler as any);
        if (index >= 0)
            list.splice(index, 1);
    }

    isOpen(): this is {
        // @ts-expect-error "Private fields are not really allowed in guards, but do work somehow"
        #socket: WebSocket
    } {
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
        this.#removeEvents();
        this.#socket?.close();
    }
}