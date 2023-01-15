import { Extension, StateEffect } from '@codemirror/state';
import { EditorView } from '@codemirror/view';
import type { StyleSpec } from 'style-mod';
import { createExtensions, createState, ExtensionSwitcher } from '../codemirror/create';
import { getText } from '../codemirror/helpers/get-text';
import { Connection } from '../protocol/connection';
import { Language, LANGUAGE_DEFAULT } from '../protocol/languages';
import type { ServerOptions } from '../protocol/messages';
import { Session, SessionEventListeners } from '../protocol/session';
import { installConnectionLossView } from './connection-loss-view';
import { ContainerRoot } from './container-root';
import { Theme, THEME_LIGHT } from './theme';

// this.#keyMap = {
//     // eslint-disable-next-line @typescript-eslint/no-floating-promises
//     'Shift-Ctrl-Space': () => { connection.sendSignatureHelpState('force'); },
// };
// this.#cm.addKeyMap(this.#keyMap);

type CreateInstanceOptions<TExtensionServerOptions, TSlowUpdateExtensionData> = {
    readonly serviceUrl: string;

    readonly language: Language | undefined;
    readonly theme: Theme | undefined;
    readonly text: string | undefined;
    readonly cursorOffset?: number | undefined;

    readonly on: {
        readonly textChange: ((getText: () => string) => void) | undefined;
    } & SessionEventListeners<TSlowUpdateExtensionData>;

    readonly disconnected: boolean | undefined;
    readonly serverOptions: TExtensionServerOptions | undefined;

    readonly codeMirror: {
        extensions: ReadonlyArray<Extension> | undefined;
        theme: { [selector: string]: StyleSpec; } | undefined;
    }
};

type InstanceContext<O, U> = {
    readonly connection: Connection<O, U>;
    readonly session: Session<O, U>;

    disconnected: boolean;
    language: Language;

    readonly codeMirror: {
        readonly view: EditorView;
        extensions: ReadonlyArray<Extension>;
        readonly extensionSwitcher: ExtensionSwitcher;
    };

    readonly root: ContainerRoot;
};

class Instance<TExtensionServerOptions, U> {
    readonly #context: InstanceContext<TExtensionServerOptions, U>;
    #connectCalled = false;

    constructor(context: InstanceContext<TExtensionServerOptions, U>) {
        this.#context = context;
    }

    getCodeMirrorView() {
        return this.#context.codeMirror.view;
    }

    getRootElement() {
        return this.#context.root.element;
    }

    getText() {
        return getText(this.#context.codeMirror.view);
    }

    setText(text: string) {
        this.#context.codeMirror.view.dispatch({
            changes: {
                from: 0,
                to: this.#context.codeMirror.view.state.doc.length,
                insert: text
            }
        });
    }

    getCursorOffset() {
        return this.#context.codeMirror.view.state.selection.main.from;
    }

    getLanguage() {
        return this.#context.language;
    }

    setLanguage(value: Language) {
        const { session, codeMirror } = this.#context;

        session.setOptions(
            ({ language: value } satisfies Partial<ServerOptions>) as Partial<ServerOptions> & Partial<TExtensionServerOptions>
        );
        codeMirror.extensions = codeMirror.extensionSwitcher.switchLanguageExtension(codeMirror.extensions, value);
        codeMirror.view.dispatch({
            effects: StateEffect.reconfigure.of(codeMirror.extensions)
        });
        this.#context.language = value;
    }

    setServerOptions(value: TExtensionServerOptions) {
        this.#context.session.setOptions(value as Partial<TExtensionServerOptions>);
    }

    setTheme(value: Theme) {
        const { root, codeMirror } = this.#context;

        codeMirror.extensions = codeMirror.extensionSwitcher.switchThemeExtension(codeMirror.extensions, value);
        codeMirror.view.dispatch({
            effects: StateEffect.reconfigure.of(codeMirror.extensions)
        });
        root.setThemeClass(value);
    }

    setServiceUrl(url: string, { disconnected }: { disconnected?: boolean } = {}) {
        this.#context.disconnected = disconnected ?? false;
        this.#connectCalled = false;
        this.#context.connection.setUrl(url, { closed: disconnected });
    }

    connect() {
        if (!this.#context.disconnected)
            throw new Error('Connect can only be called if options.disconnected was set.');
        if (this.#connectCalled)
            throw new Error('Connect can only be called once per mirrorsharp instance (on start).');

        this.#context.connection.open();
        this.#connectCalled = true;
    }

    destroy() {
        this.#context.root.destroy();
        this.#context.session.destroy();
        this.#context.connection.close();
    }
}

export const createInstance = <O, U>(container: HTMLElement, options: CreateInstanceOptions<O, U>) => {
    const {
        language = LANGUAGE_DEFAULT,
        theme = THEME_LIGHT,
        disconnected
    } = options;

    const connection = new Connection<O, U>(
        options.serviceUrl, { closed: disconnected }
    );
    const serverOptions = {
        ...({ language } satisfies ServerOptions as ServerOptions),
        ...((options.serverOptions ?? {}) satisfies Partial<O> as O)
    };
    const session = new Session<O, U>(connection, serverOptions, options.on);

    const [cmExtensions, extensionSwitcher] = createExtensions<O, U>(connection, session, {
        language,
        theme,
        themeSpec: options.codeMirror.theme ?? {},
        extraExtensions: options.codeMirror.extensions ?? [],
        onTextChange: options.on.textChange
    });
    const cmView = new EditorView({
        state: createState(cmExtensions, {
            text: options.text,
            cursorOffset: options.cursorOffset
        })
    });

    const root = new ContainerRoot(container, cmView.dom, theme);
    installConnectionLossView(root, connection);

    const instance = new Instance<O, U>({
        connection,
        session,

        disconnected: disconnected ?? false,
        language,

        codeMirror: {
            view: cmView,
            extensions: cmExtensions,
            extensionSwitcher
        },

        root
    });

    return instance;
};