import { Extension, StateEffect } from '@codemirror/state';
import { EditorView } from '@codemirror/view';
import { createExtensions, createState, ExtensionSwitcher } from '../codemirror/create';
import type { Connection } from '../protocol/connection';
import { type Language, LANGUAGE_DEFAULT } from '../protocol/languages';
import type { Message, ServerOptions } from '../protocol/messages';
import type { Session } from '../protocol/session';
import { Theme, THEME_LIGHT } from './theme';

export type EditorOptions<TExtensionServerOptions> = {
    readonly language: Language | undefined;
    readonly text: string | undefined;
    readonly cursorOffset: number | undefined;
    readonly theme: Theme | undefined;

    readonly on: {
        readonly textChange: ((getText: () => string) => void) | undefined;
        readonly serverError: ((message: string) => void) | undefined;
    };

    readonly serverOptions: TExtensionServerOptions | undefined;
    readonly codeMirror: {
        extensions?: ReadonlyArray<Extension>;
    };
};

export class Editor<TExtensionServerOptions, TSlowUpdateExtensionData> {
    readonly #connection: Connection<TExtensionServerOptions, TSlowUpdateExtensionData>;
    readonly #session: Session<TExtensionServerOptions, TSlowUpdateExtensionData>;
    readonly #options: EditorOptions<TExtensionServerOptions>;

    readonly #wrapper: HTMLElement;
    readonly #cmView: EditorView;
    #cmExtensions: ReadonlyArray<Extension>;
    readonly #extensionSwitcher: ExtensionSwitcher;

    readonly #removeConnectionListeners: () => void;

    #language: Language;
    #serverOptions: ServerOptions & TExtensionServerOptions;

    constructor(
        container: HTMLElement,
        connection: Connection<TExtensionServerOptions, TSlowUpdateExtensionData>,
        session: Session<TExtensionServerOptions, TSlowUpdateExtensionData>,
        options: EditorOptions<TExtensionServerOptions>
    ) {
        this.#connection = connection;
        this.#session = session;

        const language = options.language ?? LANGUAGE_DEFAULT;
        const theme = options.theme ?? THEME_LIGHT;
        this.#options = options;

        // const cmOptions = {
        //     gutters: [],
        //     indentUnit: 4,
        //     ...options.forCodeMirror,
        //     lineSeparator,
        //     // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        //     mode: languageModes[options.language!],
        //     lint: { async: true, getAnnotations: this.#lintGetAnnotations, hasGutters: true },
        //     lintFix: { getFixes: this.#getLintFixes },
        //     infotip: { async: true, delay: 500, getInfo: this.#infotipGetInfo, render: renderInfotip }
        // } as CodeMirror.EditorConfiguration & { lineSeparator: string };

        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        this.#language = language;
        this.#serverOptions = {
            ...(options.serverOptions ?? {}),
            language: this.#language
        } as ServerOptions & TExtensionServerOptions;

        this.#session.setOptions(this.#serverOptions);

        // const cmSource = (function getCodeMirror() {
        //     const next = textarea.nextSibling as { CodeMirror?: CodeMirror.EditorFromTextArea };
        //     if (next?.CodeMirror) {
        //         const existing = next.CodeMirror;
        //         for (const key in cmOptions) {
        //             // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-call
        //             existing.setOption(key as any, cmOptions[key as keyof typeof cmOptions]);
        //         }
        //         return { cm: existing, existing: true };
        //     }

        //     return { cm: CodeMirror.fromTextArea(textarea, cmOptions) };
        // })();

        // this.#cm = cmSource.cm;
        // this.#keyMap = {
        //     /* eslint-disable object-shorthand */
        //     'Tab': () => {
        //         if (this.#cm.somethingSelected()) {
        //             this.#cm.execCommand('indentMore');
        //             return;
        //         }
        //         this.#cm.replaceSelection('    ', 'end');
        //     },
        //     'Shift-Tab': 'indentLess',
        //     // eslint-disable-next-line @typescript-eslint/no-floating-promises
        //     'Ctrl-Space': () => { connection.sendCompletionState('force'); },
        //     // eslint-disable-next-line @typescript-eslint/no-floating-promises
        //     'Shift-Ctrl-Space': () => { connection.sendSignatureHelpState('force'); },
        //     'Ctrl-.': 'lintFixShow',
        //     'Shift-Ctrl-Y': selfDebug ? () => {
        //         // eslint-disable-next-line @typescript-eslint/no-floating-promises
        //         selfDebug.requestData(connection);
        //     } : false
        //     /* eslint-enable object-shorthand */
        // };
        // this.#cm.addKeyMap(this.#keyMap);

        this.#wrapper = document.createElement('div');
        this.#wrapper.classList.add('mirrorsharp');
        this.#setThemeClass(theme);
        container.appendChild(this.#wrapper);
        [this.#cmExtensions, this.#extensionSwitcher] = createExtensions(this.#connection, this.#session, {
            language,
            theme,
            extraExtensions: options.codeMirror.extensions,
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            onTextChange: options.on.textChange && (() => options.on.textChange!(() => this.getText()))
        });
        this.#cmView = new EditorView({
            state: createState(this.#cmExtensions, {
                text: options.text,
                cursorOffset: options.cursorOffset
            })
        });

        this.#wrapper.appendChild(this.#cmView.dom);

        this.#removeConnectionListeners = connection.addEventListeners({
            open: this.#onConnectionOpen,
            message: this.#onConnectionMessage,
            close: this.#onConnectionClose
        });
    }

    #onConnectionOpen = () => {
        this.#hideConnectionLoss();
    };

    #onConnectionMessage = (message: Message<TExtensionServerOptions, TSlowUpdateExtensionData>) => {
        switch (message.type) {
            case 'error':
                if (this.#options.on.serverError) {
                    this.#options.on.serverError(message.message);
                }
                else {
                    throw new Error(message.message);
                }
                break;
        }
    };

    #onConnectionClose = () => {
        this.#showConnectionLoss();
    };

    #connectionLossElement: HTMLDivElement|undefined;

    #showConnectionLoss = () => {
        if (!this.#connectionLossElement) {
            const connectionLossElement = document.createElement('div');
            connectionLossElement.setAttribute('class', 'mirrorsharp-connection-issue');
            connectionLossElement.innerText = 'Server connection lost, reconnecting…';
            this.#wrapper.appendChild(connectionLossElement);
            this.#connectionLossElement = connectionLossElement;
        }

        this.#wrapper.classList.add('mirrorsharp-connection-has-issue');
    };

    #hideConnectionLoss = () => {
        this.#wrapper.classList.remove('mirrorsharp-connection-has-issue');
    };

    getCodeMirrorView() {
        return this.#cmView;
    }

    getRootElement() {
        return this.#wrapper;
    }

    getText() {
        return this.#cmView.state.sliceDoc();
    }

    // setText(text: string) {
    //     this.#cm.setValue(text.replace(/(\r\n|\r|\n)/g, '\r\n'));
    // }

    setText(text: string) {
        this.#cmView.dispatch({
            changes: {
                from: 0,
                to: this.#cmView.state.doc.length,
                insert: text
            }
        });
    }

    getCursorOffset() {
        return this.#cmView.state.selection.main.from;
    }

    getLanguage() {
        return this.#language;
    }

    setLanguage(value: Language) {
        this.#session.setOptions(
            ({ language: value } satisfies Partial<ServerOptions>) as Partial<ServerOptions> & Partial<TExtensionServerOptions>
        );
        this.#cmExtensions = this.#extensionSwitcher.switchLanguageExtension(this.#cmExtensions, value);
        this.#cmView.dispatch({
            effects: StateEffect.reconfigure.of(this.#cmExtensions)
        });
    }

    setServerOptions(value: TExtensionServerOptions) {
        this.#session.setOptions(value as Partial<TExtensionServerOptions>);
    }

    #setThemeClass(theme: Theme) {
        this.#wrapper.classList.remove('mirrorsharp-theme-light', 'mirrorsharp-theme-dark');
        this.#wrapper.classList.add(`mirrorsharp-theme-${theme}`);
    }

    setTheme(value: Theme) {
        this.#cmExtensions = this.#extensionSwitcher.switchThemeExtension(this.#cmExtensions, value);
        this.#cmView.dispatch({
            effects: StateEffect.reconfigure.of(this.#cmExtensions)
        });
        this.#setThemeClass(value);
    }

    destroy(destroyOptions: { readonly keepCodeMirror?: boolean } = {}) {
        // this.#cm.save();
        this.#removeConnectionListeners();
        if (!destroyOptions.keepCodeMirror) {
            // this.#cm.toTextArea();
            return;
        }
        // this.#cm.removeKeyMap(this.#keyMap);
        // this.#removeCodeMirrorEvents();
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        // this.#cm.setOption('lint', null!);
        // this.#cm.setOption('lintFix', null);
        // TODO: fix in infotip
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        // this.#cm.setOption('infotip', null!);
    }
}