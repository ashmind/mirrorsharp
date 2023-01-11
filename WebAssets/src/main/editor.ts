import { Extension, StateEffect } from '@codemirror/state';
import { EditorView } from '@codemirror/view';
import { createExtensions, createState } from '../codemirror/create';
import { switchLanguageExtension } from '../codemirror/languages';
import { addEvents } from '../helpers/add-events';
import type { SlowUpdateOptions } from '../interfaces/slow-update';
import { Theme, THEME_LIGHT } from '../interfaces/theme';
import type { Connection } from '../protocol/connection';
import { type Language, LANGUAGE_DEFAULT } from '../protocol/languages';
import type {
    Message,
    SlowUpdateMessage,
    DiagnosticSeverity,
    ServerOptions
} from '../protocol/messages';
import type { Session } from '../protocol/session';

type EditorOptions<TExtensionServerOptions, TSlowUpdateExtensionData> = {
    readonly language?: Language | undefined;
    readonly initialText?: string | undefined;
    readonly initialCursorOffset?: number | undefined;
    readonly theme?: Theme | undefined;

    readonly on?: ({
        readonly textChange?: (getText: () => string) => void;
        readonly connectionChange?: {
            (event: 'open', e: Event): void;
            (event: 'error', e: ErrorEvent): void;
            (event: 'close', e: CloseEvent): void;
        };
        readonly serverError?: (message: string) => void;
    } & SlowUpdateOptions<TSlowUpdateExtensionData>) | undefined;

    readonly initialServerOptions?: TExtensionServerOptions | undefined;
};

export class Editor<TExtensionServerOptions, TSlowUpdateExtensionData> {
    readonly #connection: Connection<TExtensionServerOptions, TSlowUpdateExtensionData>;
    readonly #session: Session<TExtensionServerOptions>;
    readonly #options: EditorOptions<TExtensionServerOptions, TSlowUpdateExtensionData>;

    readonly #wrapper: HTMLElement;
    readonly #cmView: EditorView;
    #cmExtensions: ReadonlyArray<Extension>;

    // readonly #removeCodeMirrorEvents: () => void;
    readonly #removeConnectionEvents: () => void;

    #language: Language;
    #serverOptions: ServerOptions & TExtensionServerOptions;

    constructor(
        container: HTMLElement,
        connection: Connection<TExtensionServerOptions, TSlowUpdateExtensionData>,
        session: Session<TExtensionServerOptions>,
        options: EditorOptions<TExtensionServerOptions, TSlowUpdateExtensionData>
    ) {
        this.#connection = connection;
        this.#session = session;

        options = {
            language: LANGUAGE_DEFAULT,
            ...options,
            on: {
                slowUpdateWait:   () => ({}),
                slowUpdateResult: () => ({}),
                textChange:       () => ({}),
                connectionChange: () => ({}),
                serverError:      (message: string) => { throw new Error(message); },
                ...options.on
            }
        };
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
        this.#language = options.language!;
        this.#serverOptions = {
            ...(options.initialServerOptions ?? {}),
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

        const theme = options.theme ?? THEME_LIGHT;

        this.#wrapper = document.createElement('div');
        this.#wrapper.classList.add('mirrorsharp', `mirrorsharp-theme-${theme}`);
        container.appendChild(this.#wrapper);
        this.#cmExtensions = createExtensions(this.#connection, this.#session, {
            initialLanguage: this.#language,
            theme
        });
        this.#cmView = new EditorView({
            state: createState(this.#cmExtensions, {
                initialText: options.initialText,
                initialCursorOffset: options.initialCursorOffset
            })
        });

        this.#wrapper.appendChild(this.#cmView.dom);

        this.#removeConnectionEvents = addEvents(connection, {
            open: this.#onConnectionOpen,
            message: this.#onConnectionMessage,
            error: this.#onConnectionCloseOrError,
            close: this.#onConnectionCloseOrError
        });

        // this.#removeCodeMirrorEvents = addEvents(this.#cm, {
        //     beforeChange: this.#onCodeMirrorBeforeChange,
        //     cursorActivity: this.#onCodeMirrorCursorActivity,
        //     changes: this.#onCodeMirrorChanges
        // });
    }

    #onConnectionOpen = (e: Event) => {
        this.#hideConnectionLoss();
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        this.#options.on!.connectionChange!('open', e);
    };

    #onConnectionMessage = (message: Message<TExtensionServerOptions, TSlowUpdateExtensionData>) => {
        switch (message.type) {
            case 'slowUpdate':
                this.#showSlowUpdate(message);
                break;

            case 'error':
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                this.#options.on!.serverError!(message.message);
                break;
        }
    };

    #onConnectionCloseOrError = (e: CloseEvent|ErrorEvent) => {
        this.#showConnectionLoss();
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        const connectionChange = this.#options.on!.connectionChange!;
        if (e instanceof CloseEvent) {
            connectionChange('close', e);
        }
        else {
            connectionChange('error', e);
        }
    };

    #showSlowUpdate = (update: SlowUpdateMessage<TSlowUpdateExtensionData>) => {
        // const annotations: Array<DiagnosticAnnotation> = [];

        // Higher severities must go last -- CodeMirror uses last one for the icon.
        // Unless one is error, in which case it's always error -- but still makes
        // sense to handle this consistently.
        const priorityBySeverity = { hidden: 0, info: 1, warning: 2, error: 3 };
        const diagnostics = update.diagnostics.slice(0);
        diagnostics.sort((a, b) => {
            const aOrder = priorityBySeverity[a.severity];
            const bOrder = priorityBySeverity[b.severity];
            return aOrder !== bOrder ? (aOrder > bOrder ? 1 : -1) : 0;
        });

        for (const diagnostic of diagnostics) {
            let severity: DiagnosticSeverity|'unnecessary' = diagnostic.severity;
            const isUnnecessary = diagnostic.tags.includes('unnecessary');
            if (severity === 'hidden' && !isUnnecessary)
                continue;

            if (isUnnecessary && (severity === 'hidden' || severity === 'info'))
                severity = 'unnecessary';

            // const range = this.#spanToRange(diagnostic.span);
            // annotations.push({
            //     severity,
            //     message: diagnostic.message,
            //     from: range.from,
            //     to: range.to,
            //     diagnostic
            // });
        }
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        // this.#capturedUpdateLinting(this.#cm, annotations);
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        this.#options.on!.slowUpdateResult!({
            diagnostics: update.diagnostics,
            x: update.x
        });
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
        this.#cmExtensions = switchLanguageExtension(this.#cmExtensions, value);
        this.#cmView.dispatch({
            effects: StateEffect.reconfigure.of(this.#cmExtensions)
        });
    }

    setServerOptions(value: TExtensionServerOptions) {
        this.#session.setOptions(value as Partial<TExtensionServerOptions>);
    }

    destroy(destroyOptions: { readonly keepCodeMirror?: boolean } = {}) {
        // this.#cm.save();
        this.#removeConnectionEvents();
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