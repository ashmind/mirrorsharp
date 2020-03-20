import type { SelfDebug } from '../interfaces/self-debug';
import type {
    Connection as ConnectionInterface,
    StateCommand,
    ConnectionOpenHandler,
    ConnectionErrorHandler,
    ConnectionMessageHandler,
    ConnectionCloseHandler,
    ConnectionEventMap
} from '../interfaces/connection';
import { addEvents } from '../helpers/add-events';

function Connection<TExtensionData>(this: ConnectionInterface<TExtensionData>, url: string, selfDebug: SelfDebug<TExtensionData>|null): void {
    let socket: WebSocket;
    let openPromise: Promise<void>;
    const eventKeys = ['open', 'message', 'error', 'close'] as const;
    const handlers = {
        open:    [] as Array<ConnectionOpenHandler>,
        message: [] as Array<ConnectionMessageHandler<TExtensionData>>,
        error:   [] as Array<ConnectionErrorHandler>,
        close:   [] as Array<ConnectionCloseHandler>
    } as const;

    open();

    let mustBeClosed = false;
    let reopenPeriod = 0;
    let reopenPeriodResetTimer: ReturnType<typeof setTimeout>|null;
    let reopening = false;

    function open() {
        socket = new WebSocket(url);
        openPromise = new Promise(resolve => {
            socket.addEventListener('open', () => {
                reopenPeriodResetTimer = setTimeout(() => { reopenPeriod = 0; }, reopenPeriod);
                resolve();
            });
        });

        for (const key of eventKeys) {
            const handlersByKey = handlers[key];
            socket.addEventListener(key, e => {
                const handlerArguments = [e];
                if (key === 'message') {
                    const data = JSON.parse((e as MessageEvent).data);
                    if (data.type === 'self:debug') {
                        for (const entry of data.log) {
                            entry.time = new Date(entry.time);
                        }
                    }
                    if (selfDebug)
                        selfDebug.log('before', JSON.stringify(data));
                    handlerArguments.unshift(data);
                }
                for (const handler of handlersByKey) {
                    // eslint-disable-next-line @typescript-eslint/no-explicit-any
                    (handler as any)(...handlerArguments);
                }
                if (selfDebug && key === 'message')
                    selfDebug.log('after', JSON.stringify(handlerArguments[0]));
            });
        }
    }

    function tryToReopen() {
        if (mustBeClosed || reopening)
            return;

        if (reopenPeriodResetTimer) {
            clearTimeout(reopenPeriodResetTimer);
            reopenPeriodResetTimer = null;
        }

        reopening = true;
        setTimeout(() => {
            open();
            reopening = false;
        }, reopenPeriod);
        if (reopenPeriod < 60000)
            reopenPeriod = Math.min(5 * (reopenPeriod + 200), 60000);
    }

    async function sendWhenOpen(command: string) {
        if (mustBeClosed)
            throw `Cannot send command '${command}' after the close() call.`;

        await openPromise;
        if (selfDebug)
            selfDebug.log('send', command);
        socket.send(command);
    }

    this.on = function<TKey extends keyof ConnectionEventMap<TExtensionData>>(key: TKey, handler: ConnectionEventMap<TExtensionData>[TKey]) {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        handlers[key].push(handler as any);
    };

    this.off = function<TKey extends keyof ConnectionEventMap<TExtensionData>>(key: TKey, handler: ConnectionEventMap<TExtensionData>[TKey]) {
        const list = handlers[key];
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const index = list.indexOf(handler as any);
        if (index >= 0)
            list.splice(index, 1);
    };

    const removeEvents = addEvents(this, {
        error: tryToReopen,
        close: tryToReopen
    });

    this.sendReplaceText = function(
        start: number,
        length: number,
        newText: string,
        cursorIndexAfter: number,
        reason: string
    ) {
        return sendWhenOpen('R' + start + ':' + length + ':' + cursorIndexAfter + ':' + (reason || '') + ':' + newText);
    };

    this.sendMoveCursor = function(cursorIndex: number) {
        return sendWhenOpen('M' + cursorIndex);
    };

    this.sendTypeChar = function(char: string) {
        return sendWhenOpen('C' + char);
    };

    const stateCommandMap: Readonly<{
        cancel: 'X';
        force: 'F';
        [key: number]: undefined;
    }> = { cancel: 'X', force: 'F' };

    this.sendCompletionState = function(indexOrCommand: StateCommand|'info'|number, indexIfInfo?: number) {
        const argument = indexOrCommand !== 'info'
            ? (stateCommandMap[indexOrCommand] || indexOrCommand)
            : 'I' + indexIfInfo;
        return sendWhenOpen('S' + argument);
    };

    this.sendSignatureHelpState = function(command: StateCommand) {
        return sendWhenOpen('P' + stateCommandMap[command]);
    };

    this.sendRequestInfoTip = function(cursorIndex: number) {
        return sendWhenOpen('I' + cursorIndex);
    };

    this.sendSlowUpdate = function() {
        return sendWhenOpen('U');
    };

    this.sendApplyDiagnosticAction = function(actionId: number) {
        return sendWhenOpen('F' + actionId);
    };

    this.sendSetOptions = function(options: { [key: string]: unknown }) {
        const optionPairs = [];
        for (const key in options) {
            optionPairs.push(key + '=' + options[key]);
        }
        return sendWhenOpen('O' + optionPairs.join(','));
    };

    this.sendRequestSelfDebugData = function() {
        return sendWhenOpen('Y');
    };

    this.close = function(): void {
        mustBeClosed = true;
        removeEvents();
        socket.close();
    };
}

const ConnectionAsConstructor = Connection as unknown as {
    new<TExtensionData>(url: string, selfDebug: SelfDebug<TExtensionData>|null): ConnectionInterface<TExtensionData>;
};

export { ConnectionAsConstructor as Connection };