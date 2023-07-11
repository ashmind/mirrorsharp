import type { Connection, ReplaceTextCommand } from './connection';
import { LANGUAGE_DEFAULT } from './languages';
import type { DiagnosticSeverity, ErrorMessage, Message, ServerOptions, ServerPosition, SlowUpdateMessage } from './messages';

const UPDATE_PERIOD = 500;

type FullTextContext = {
    readonly getText: () => string;
    readonly getCursorIndex: () => ServerPosition;
};

type SlowUpdateResultDiagnostic = {
    readonly id: string;
    readonly severity: DiagnosticSeverity;
    readonly message: string;
};

type SlowUpdateResult<TSlowUpdateExtensionData> = {
    readonly diagnostics: ReadonlyArray<SlowUpdateResultDiagnostic>;
    readonly extensionResult: TSlowUpdateExtensionData;
};

export type SessionEventListeners<TSlowUpdateExtensionData> = {
    readonly connectionChange: ((event: 'open' | 'loss') => void) | undefined;
    readonly slowUpdateWait: (() => void) | undefined;
    readonly slowUpdateResult: ((args: SlowUpdateResult<TSlowUpdateExtensionData>) => void) | undefined;
    readonly serverError: ((message: string) => void) | undefined;
};

// Defaults are 'unknown' rather than 'void', as it exists for internal convenience,
// and we assume in most cases this is not 'void'. Anything public should have 'void' though.
export class Session<TExtensionServerOptions = unknown, TSlowUpdateExtensionData = unknown> {
    readonly #connection: Connection<TExtensionServerOptions, TSlowUpdateExtensionData>;
    readonly #slowUpdateTimer: ReturnType<typeof setTimeout>;
    readonly #removeConnectionEvents: () => void;
    readonly #on: SessionEventListeners<TSlowUpdateExtensionData>;

    #textSent = false;
    #hadChangesSinceLastSlowUpdate = false;

    #fullOptions: ServerOptions & Partial<TExtensionServerOptions>;
    #fullTextContext?: FullTextContext;

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    constructor(
        connection: Connection<TExtensionServerOptions, TSlowUpdateExtensionData>,
        serverOptions: ServerOptions & TExtensionServerOptions,
        on: SessionEventListeners<TSlowUpdateExtensionData>
    ) {
        this.#connection = connection;
        this.#removeConnectionEvents = connection.addEventListeners({
            // eslint-disable-next-line @typescript-eslint/no-misused-promises
            open: () => {
                on.connectionChange?.('open');
                this.#resendAllOnOpen();
            },
            message: e => this.#receiveMessage(e),
            close: () => {
                on.connectionChange?.('loss');
            }
        });
        this.#on = on;
        this.#slowUpdateTimer = setInterval(() => this.#requestSlowUpdate(), UPDATE_PERIOD);
        this.#fullOptions = serverOptions;
    }

    #resendAllOnOpen() {
        this.#textSent = false;
        if (!this.#areFullOptionsDefault())
            this.#sendSetOptions(this.#fullOptions);
        if (this.#fullTextContext)
            this.#sendFullText(this.#fullTextContext);
        this.#requestSlowUpdate();
    }

    setOptions(options: Partial<ServerOptions> & Partial<TExtensionServerOptions>) {
        this.#fullOptions = { ...this.#fullOptions, ...options };
        if (this.#connection.isOpen()) {
            this.#sendSetOptions(options);
            this.#requestSlowUpdate();
        }
    }

    #sendSetOptions(options: Partial<ServerOptions> & Partial<TExtensionServerOptions>) {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        this.#connection.sendSetOptions(options);
        this.#hadChangesSinceLastSlowUpdate = true;
    }

    #areFullOptionsDefault = () => {
        const keys = Object.keys(this.#fullOptions);
        return keys.length === 1
            && keys[0] === 'language'
            && this.#fullOptions.language === LANGUAGE_DEFAULT;
    };

    setFullText(context: FullTextContext) {
        this.#fullTextContext = context;
        if (this.#connection.isOpen())
            this.#sendFullText(context);
    }

    #sendFullText({ getText, getCursorIndex }: FullTextContext) {
        const text = getText();
        if (text.length === 0)
            return;

        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        this.#connection.sendReplaceText({
            start: 0 as unknown as ServerPosition,
            length: 0,
            newText: getText(),
            cursorIndexAfter: getCursorIndex()
        });
        this.#textSent = true;
        this.#hadChangesSinceLastSlowUpdate = true;
    }

    sendPartialText(command: ReplaceTextCommand) {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        this.#connection.sendReplaceText(command);
        this.#hadChangesSinceLastSlowUpdate = true;
        this.#textSent = true;
    }

    sendTypeChar(char: string) {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        this.#connection.sendTypeChar(char);
        this.#hadChangesSinceLastSlowUpdate = true;
        this.#textSent = true;
    }

    sendMoveCursor(cursorIndex: ServerPosition) {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        this.#connection.sendMoveCursor(cursorIndex);
    }

    #requestSlowUpdate() {
        if (!this.#connection.isOpen())
            return;

        if (!this.#hadChangesSinceLastSlowUpdate)
            return;

        if (!this.#textSent)
            return;

        this.#on.slowUpdateWait?.();
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        this.#connection.sendSlowUpdate();
        this.#hadChangesSinceLastSlowUpdate = false;
    }

    #receiveSlowUpdate(message: SlowUpdateMessage<TSlowUpdateExtensionData>) {
        if (!this.#on.slowUpdateResult)
            return;

        const diagnostics = message.diagnostics.map(
            ({ id, message, severity }) => ({ id, message, severity })
        );
        this.#on.slowUpdateResult({
            diagnostics,
            extensionResult: message.x
        });
    }

    #receiveServerError({ message }: ErrorMessage) {
        if (!this.#on.serverError)
            throw new Error(message);
        this.#on.serverError(message);
    }

    #receiveMessage(message: Message<TExtensionServerOptions, TSlowUpdateExtensionData>) {
        switch (message.type) {
            case 'optionsEcho':
                this.#fullOptions = { ...this.#fullOptions, ...message.options };
                break;

            case 'slowUpdate':
                this.#receiveSlowUpdate(message);
                break;

            case 'error':
                this.#receiveServerError(message);
                break;
        }
    }

    destroy() {
        this.#removeConnectionEvents();
        clearInterval(this.#slowUpdateTimer);
    }
}