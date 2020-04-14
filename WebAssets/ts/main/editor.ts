import type {
    Message,
    ChangeData,
    DiagnosticData,
    SlowUpdateMessage,
    DiagnosticSeverity,
    ServerOptions,
    SpanData
} from '../interfaces/protocol';
import type { Connection } from '../interfaces/connection';
import type { SelfDebug } from '../interfaces/self-debug';
import type { Editor as EditorInterface, DestroyOptions, EditorOptions } from '../interfaces/editor';
import CodeMirror from 'codemirror';
import 'codemirror/mode/clike/clike';
import 'codemirror-addon-infotip';
import 'codemirror-addon-lint-fix';
import { renderInfotip } from './render-infotip';
import { Hinter } from './hinter';
import { SignatureTip } from './signature-tip';
import { addEvents } from '../helpers/add-events';

const indexKey = '$mirrorsharp-index';
interface PositionWithIndex extends CodeMirror.Position {
    [indexKey]: number;
}

interface DiagnosticAnnotation extends CodeMirror.Annotation {
    readonly diagnostic: DiagnosticData;
}

interface AnnotationFixWithId extends CodeMirror.AnnotationFix {
    readonly id: number;
}

function Editor<TExtensionServerOptions, TSlowUpdateExtensionData>(
    this: EditorInterface<TExtensionServerOptions>,
    textarea: HTMLTextAreaElement,
    connection: Connection<TExtensionServerOptions, TSlowUpdateExtensionData>,
    selfDebug: SelfDebug<TExtensionServerOptions, TSlowUpdateExtensionData>|null,
    options: EditorOptions<TExtensionServerOptions, TSlowUpdateExtensionData>
) {
    type ExtendedServerOptions = ServerOptions & TExtensionServerOptions;

    const lineSeparator = '\r\n';
    const defaultLanguage = 'C#';
    const languageModes = {
        'C#': 'text/x-csharp',
        'Visual Basic': 'text/x-vb',
        'F#': 'text/x-fsharp',
        'PHP': 'application/x-httpd-php'
    };

    let lintingSuspended = true;
    let hadChangesSinceLastLinting = false;
    let capturedUpdateLinting: CodeMirror.UpdateLintingCallback|null|undefined;

    options = { language: defaultLanguage, ...options };
    options.on = {
        slowUpdateWait:   () => ({}),
        slowUpdateResult: () => ({}),
        textChange:       () => ({}),
        connectionChange: () => ({}),
        serverError:      (message: string) => { throw new Error(message); },
        ...options.on
    };

    const cmOptions = {
        gutters: [],
        indentUnit: 4,
        ...options.forCodeMirror,
        lineSeparator,
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        mode: languageModes[options.language!],
        lint: { async: true, getAnnotations: lintGetAnnotations, hasGutters: true },
        lintFix: { getFixes },
        infotip: { async: true, delay: 500, getInfo: infotipGetInfo, render: renderInfotip }
    } as CodeMirror.EditorConfiguration & { lineSeparator: string };

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    cmOptions.gutters!.push('CodeMirror-lint-markers');

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    let language = options.language!;
    let serverOptions = { ...(options.initialServerOptions ?? {}), language } as ExtendedServerOptions;

    const cmSource = (function getCodeMirror() {
        const next = textarea.nextSibling as { CodeMirror?: CodeMirror.EditorFromTextArea };
        if (next?.CodeMirror) {
            const existing = next.CodeMirror;
            for (const key in cmOptions) {
                // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-call
                existing.setOption(key as any, cmOptions[key as keyof typeof cmOptions]);
            }
            return { cm: existing, existing: true };
        }

        return { cm: CodeMirror.fromTextArea(textarea, cmOptions) };
    })();

    const cm = cmSource.cm;
    const keyMap = {
        /* eslint-disable object-shorthand */
        'Tab': function() {
            if (cm.somethingSelected()) {
                cm.execCommand('indentMore');
                return;
            }
            cm.replaceSelection('    ', 'end');
        },
        'Shift-Tab': 'indentLess',
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        'Ctrl-Space': () => { connection.sendCompletionState('force'); },
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        'Shift-Ctrl-Space': () => { connection.sendSignatureHelpState('force'); },
        'Ctrl-.': 'lintFixShow',
        'Shift-Ctrl-Y': selfDebug ? () => {
            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            selfDebug.requestData(connection);
        } : false
        /* eslint-enable object-shorthand */
    } as const;
    cm.addKeyMap(keyMap);
    // see https://github.com/codemirror/CodeMirror/blob/dbaf6a94f1ae50d387fa77893cf6b886988c2147/addon/lint/lint.js#L133
    // ensures that next 'id' will be -1 whether a change happened or not
    // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
    cm.state.lint.waitingFor = -2;
    if (!cmSource.existing)
        setText(textarea.value);

    const getText = cm.getValue.bind(cm);
    if (selfDebug)
        selfDebug.watchEditor(getText, getCursorIndex);

    const cmWrapper = cm.getWrapperElement();
    cmWrapper.classList.add('mirrorsharp', 'mirrorsharp-theme');

    const hinter = new Hinter(cm, connection);
    const signatureTip = new SignatureTip(cm);
    const removeConnectionEvents = addEvents(connection, {
        open(e: Event) {
            hideConnectionLoss();
            if (!areDefaultServerOptions(serverOptions)) {
                // eslint-disable-next-line @typescript-eslint/no-floating-promises
                connection.sendSetOptions(serverOptions);
            }

            const text = cm.getValue();
            if (text === '' || text == null) {
                lintingSuspended = false;
                return;
            }

            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            connection.sendReplaceText(0, 0, text, getCursorIndex());
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            options.on!.connectionChange!('open', e);
            lintingSuspended = false;
            hadChangesSinceLastLinting = true;
            if (capturedUpdateLinting) {
                // eslint-disable-next-line @typescript-eslint/no-floating-promises
                requestSlowUpdate();
            }
        },
        message: onMessage,
        error: onCloseOrError,
        close: onCloseOrError
    });

    function onCloseOrError(e: CloseEvent|ErrorEvent) {
        lintingSuspended = true;
        showConnectionLoss();
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        const connectionChange = options.on!.connectionChange!;
        if (e instanceof CloseEvent) {
            connectionChange('close', e);
        }
        else {
            connectionChange('error', e);
        }
    }

    let changePending = false;
    let changeReason: string|null = null;
    let changesAreFromServer = false;
    const removeCMEvents = addEvents(cm, {
        beforeChange(_: CodeMirror.Editor, change: CodeMirror.EditorChangeCancellable) {
            (change.from as PositionWithIndex)[indexKey] = cm.indexFromPos(change.from);
            (change.to as PositionWithIndex)[indexKey] = cm.indexFromPos(change.to);
            changePending = true;
        },

        cursorActivity() {
            if (changePending)
                return;
            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            connection.sendMoveCursor(getCursorIndex());
        },

        changes(_: CodeMirror.Editor, changes: ReadonlyArray<CodeMirror.EditorChange>) {
            hadChangesSinceLastLinting = true;
            changePending = false;
            const cursorIndex = getCursorIndex();
            changes = mergeChanges(changes);
            for (let i = 0; i < changes.length; i++) {
                const change = changes[i];
                const start = (change.from as PositionWithIndex)[indexKey];
                const length = (change.to as PositionWithIndex)[indexKey] - start;
                const text = change.text.join(lineSeparator);
                if (cursorIndex === start + 1 && text.length === 1 && !changesAreFromServer) {
                    if (length > 0) {
                        // eslint-disable-next-line @typescript-eslint/no-floating-promises
                        connection.sendReplaceText(start, length, '', cursorIndex - 1);
                    }
                    // eslint-disable-next-line @typescript-eslint/no-floating-promises
                    connection.sendTypeChar(text);
                }
                else {
                    // eslint-disable-next-line @typescript-eslint/no-floating-promises
                    connection.sendReplaceText(start, length, text, cursorIndex, changeReason);
                }
            }
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            options.on!.textChange!(getText);
        }
    });

    function mergeChanges(changes: ReadonlyArray<CodeMirror.EditorChange>) {
        if (changes.length < 2)
            return changes;
        const results = [];
        let before: CodeMirror.EditorChange|null = null;
        for (const change of changes) {
            if (changesCanBeMerged(before, change)) {
                before = {
                    /* eslint-disable @typescript-eslint/no-non-null-assertion */
                    from: before!.from,
                    to: before!.to,
                    text: [before!.text[0] + change.text[0]],
                    origin: change.origin
                    /* eslint-enable @typescript-eslint/no-non-null-assertion */
                };
            }
            else {
                if (before)
                    results.push(before);
                before = change;
            }
        }
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        results.push(before!);
        return results;
    }

    function changesCanBeMerged(first: CodeMirror.EditorChange|null, second: CodeMirror.EditorChange|null) {
        return first && second
            && first.origin === 'undo'
            && second.origin === 'undo'
            && first.to.line === second.from.line
            && first.text.length === 1
            && second.text.length === 1
            && second.from.ch === second.to.ch
            && (first.to.ch + first.text[0].length) === second.from.ch;
    }

    function onMessage(message: Message<TExtensionServerOptions, TSlowUpdateExtensionData>) {
        switch (message.type) {
            case 'changes':
                receiveServerChanges(message.changes, message.reason);
                break;

            case 'completions':
                hinter.start(message.completions, message.span, {
                    commitChars: message.commitChars,
                    suggestion: message.suggestion
                });
                break;

            case 'completionInfo':
                hinter.showTip(message.index, message.parts);
                break;

            case 'signatures':
                signatureTip.update(message);
                break;

            case 'infotip':
                if (!message.sections) {
                    cm.infotipUpdate(null);
                    return;
                }
                cm.infotipUpdate({
                    data: message,
                    range: spanToRange(message.span)
                });
                break;

            case 'slowUpdate':
                showSlowUpdate(message);
                break;

            case 'optionsEcho':
                receiveServerOptions(message.options);
                break;

            case 'self:debug':
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                selfDebug!.displayData(message);
                break;

            case 'error':
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                options.on!.serverError!(message.message);
                break;

            default:
                throw new Error('Unknown message type "' + message.type);
        }
    }

    function lintGetAnnotations(_: string, updateLinting: CodeMirror.UpdateLintingCallback) {
        if (!capturedUpdateLinting) {
            capturedUpdateLinting = function(this: ThisParameterType<CodeMirror.UpdateLintingCallback>, ...args: Parameters<CodeMirror.UpdateLintingCallback>) {
                // see https://github.com/codemirror/CodeMirror/blob/dbaf6a94f1ae50d387fa77893cf6b886988c2147/addon/lint/lint.js#L133
                // ensures that next 'id' will always match 'waitingFor'
                // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
                cm.state.lint.waitingFor = -1;
                updateLinting.apply(this, args);
            };
        }
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        requestSlowUpdate();
    }

    function getCursorIndex() {
        return cm.indexFromPos(cm.getCursor());
    }

    function setText(text: string) {
        cm.setValue(text.replace(/(\r\n|\r|\n)/g, '\r\n'));
    }

    function receiveServerChanges(changes: ReadonlyArray<ChangeData>, reason: string|null) {
        changesAreFromServer = true;
        changeReason = reason ?? 'server';
        cm.operation(() => {
            let offset = 0;
            for (const change of changes) {
                const from = cm.posFromIndex(change.start + offset);
                const to = change.length > 0 ? cm.posFromIndex(change.start + offset + change.length) : from;
                cm.replaceRange(change.text, from, to, '+server');
                offset += change.text.length - change.length;
            }
        });
        changeReason = null;
        changesAreFromServer = false;
    }

    function getFixes(cm: CodeMirror.Editor, line: number, annotations: ReadonlyArray<CodeMirror.Annotation>) {
        const fixes: Array<CodeMirror.AnnotationFix> = [];
        for (const annotation of annotations) {
            const diagnostic = (annotation as DiagnosticAnnotation).diagnostic;
            if (!diagnostic.actions)
                continue;
            for (const action of diagnostic.actions) {
                fixes.push({
                    text: action.title,
                    apply: requestApplyFixAction,
                    id: action.id
                } as AnnotationFixWithId);
            }
        }
        return fixes;
    }

    function requestApplyFixAction(cm: CodeMirror.Editor, line: number, fix: AnnotationFixWithId) {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        connection.sendApplyDiagnosticAction(fix.id);
    }

    function infotipGetInfo(cm: CodeMirror.Editor, position: CodeMirror.Position) {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        connection.sendRequestInfoTip(cm.indexFromPos(position));
    }

    function requestSlowUpdate(force?: boolean) {
        if (lintingSuspended || !(hadChangesSinceLastLinting || force))
            return null;
        hadChangesSinceLastLinting = false;
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        options.on!.slowUpdateWait!();
        return connection.sendSlowUpdate();
    }

    function showSlowUpdate(update: SlowUpdateMessage<TSlowUpdateExtensionData>) {
        const annotations: Array<DiagnosticAnnotation> = [];

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

            const range = spanToRange(diagnostic.span);
            annotations.push({
                severity,
                message: diagnostic.message,
                from: range.from,
                to: range.to,
                diagnostic
            });
        }
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        capturedUpdateLinting!(cm, annotations);
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        options.on!.slowUpdateResult!({
            diagnostics: update.diagnostics,
            x: update.x
        });
    }

    let connectionLossElement: HTMLDivElement|undefined;
    function showConnectionLoss() {
        const wrapper = cm.getWrapperElement();
        if (!connectionLossElement) {
            connectionLossElement = document.createElement('div');
            connectionLossElement.setAttribute('class', 'mirrorsharp-connection-issue');
            connectionLossElement.innerText = 'Server connection lost, reconnecting…';
            wrapper.appendChild(connectionLossElement);
        }

        wrapper.classList.add('mirrorsharp-connection-has-issue');
    }

    function hideConnectionLoss() {
        cm.getWrapperElement().classList.remove('mirrorsharp-connection-has-issue');
    }

    function areDefaultServerOptions(value: ExtendedServerOptions) {
        const keys = Object.keys(value);
        return keys.length === 1
            && keys[0] === 'language'
            && value.language === defaultLanguage;
    }

    async function sendServerOptions(value: ServerOptions|Partial<ExtendedServerOptions>) {
        await connection.sendSetOptions(value);
        await requestSlowUpdate(true);
    }

    function receiveServerOptions(value: ExtendedServerOptions) {
        serverOptions = { ...serverOptions, ...value };
        // TODO: understand later
        // eslint-disable-next-line no-undefined
        if (value.language !== undefined && value.language !== language) {
            language = value.language;
            cm.setOption('mode', languageModes[language]);
        }
    }

    function spanToRange(span: SpanData) {
        return {
            from: cm.posFromIndex(span.start),
            to: cm.posFromIndex(span.start + span.length)
        };
    }

    function destroy(destroyOptions: DestroyOptions = {}) {
        cm.save();
        removeConnectionEvents();
        if (!destroyOptions.keepCodeMirror) {
            cm.toTextArea();
            return;
        }
        cm.removeKeyMap(keyMap);
        removeCMEvents();
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        cm.setOption('lint', null!);
        cm.setOption('lintFix', null);
        // TODO: fix in infotip
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        cm.setOption('infotip', null!);
    }

    this.getCodeMirror = () => cm;
    this.setText = setText;
    this.getLanguage = () => language;
    this.setLanguage = value => sendServerOptions({ language: value });
    this.setServerOptions = value => sendServerOptions(value);
    this.destroy = destroy;
}

const EditorAsConstructor = Editor as unknown as {
    new<TExtensionServerOptions, TSlowUpdateExtensionData>(
        textarea: HTMLTextAreaElement,
        connection: Connection<TExtensionServerOptions, TSlowUpdateExtensionData>,
        selfDebug: SelfDebug<TExtensionServerOptions, TSlowUpdateExtensionData>|null,
        options: EditorOptions<TExtensionServerOptions, TSlowUpdateExtensionData>
    ): EditorInterface<TSlowUpdateExtensionData>;
};

export { EditorAsConstructor as Editor };