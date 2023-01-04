import { addEvents } from '../helpers/add-events';
import { LANGUAGE_DEFAULT, Message, ServerOptions } from '../interfaces/protocol';
import type { Connection, ReplaceTextCommand } from './connection';

const UPDATE_PERIOD = 500;

type FullTextContext = {
    getText: () => string;
    getCursorIndex: () => number;
};

export class Session<TExtensionServerOptions = unknown> {
    readonly #connection: Connection<TExtensionServerOptions>;
    readonly #slowUpdateTimer: ReturnType<typeof setTimeout>;
    readonly #removeConnectionEvents: () => void;

    #textSent = false;
    #hadChangesSinceLastSlowUpdate = false;

    #fullOptions = {} as ServerOptions & Partial<TExtensionServerOptions>;
    #fullTextContext?: FullTextContext;

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    constructor(connection: Connection<TExtensionServerOptions>) {
        this.#connection = connection;
        this.#removeConnectionEvents = addEvents(connection, {
            // eslint-disable-next-line @typescript-eslint/no-misused-promises
            open: () => this.#resendAllOnOpen(),
            message: e => this.#receiveMessage(e)
        });
        this.#slowUpdateTimer = setInterval(() => this.#requestSlowUpdate(), UPDATE_PERIOD);
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
            start: 0,
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

    sendMoveCursor(cursorIndex: number) {
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

        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        this.#connection.sendSlowUpdate();
        this.#hadChangesSinceLastSlowUpdate = false;
    }

    #receiveMessage(data: Message<TExtensionServerOptions, unknown>) {
        if (data.type !== 'optionsEcho')
            return;

        this.#fullOptions = { ...this.#fullOptions, ...data.options };
    }

    destroy() {
        this.#removeConnectionEvents();
        clearInterval(this.#slowUpdateTimer);
    }
}