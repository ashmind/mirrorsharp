import { Extension, StateEffect } from '@codemirror/state';
import { EditorView } from '@codemirror/view';
import { createExtensions, createState, ExtensionSwitcher } from '../codemirror/create';
import type { Connection } from '../protocol/connection';
import { type Language, LANGUAGE_DEFAULT } from '../protocol/languages';
import type { ServerOptions } from '../protocol/messages';
import type { Session } from '../protocol/session';
import { connectionLossView } from './connection-loss-view';
import { Theme, THEME_LIGHT } from './theme';

export type EditorOptions<TExtensionServerOptions> = {
    readonly language: Language | undefined;
    readonly text: string | undefined;
    readonly cursorOffset: number | undefined;
    readonly theme: Theme | undefined;
    readonly onTextChange: ((getText: () => string) => void) | undefined;

    readonly serverOptions: TExtensionServerOptions | undefined;
    readonly codeMirror: {
        extensions?: ReadonlyArray<Extension>;
    };
};

export class Editor<TExtensionServerOptions, TSlowUpdateExtensionData> {
    readonly #connection: Connection<TExtensionServerOptions, TSlowUpdateExtensionData>;
    readonly #session: Session<TExtensionServerOptions, TSlowUpdateExtensionData>;

    readonly #wrapper: HTMLElement;
    readonly #cmView: EditorView;
    #cmExtensions: ReadonlyArray<Extension>;
    readonly #extensionSwitcher: ExtensionSwitcher;

    readonly #destroyConnectionLossView: () => void;

    #language: Language;

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

        this.#language = language;
        this.#session.setOptions({
            ...(options.serverOptions ?? {}),
            language
        } as ServerOptions & TExtensionServerOptions);

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
            onTextChange: options.onTextChange && (() => options.onTextChange!(() => this.getText()))
        });
        this.#cmView = new EditorView({
            state: createState(this.#cmExtensions, {
                text: options.text,
                cursorOffset: options.cursorOffset
            })
        });

        this.#wrapper.appendChild(this.#cmView.dom);

        this.#destroyConnectionLossView = connectionLossView(this.#wrapper, connection);
    }

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
        this.#destroyConnectionLossView();
        if (!destroyOptions.keepCodeMirror) {
            // this.#cm.toTextArea();
            return;
        }
    }
}