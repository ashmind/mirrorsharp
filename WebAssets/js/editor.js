/* globals assign:false, CodeMirror:false, Hinter:false, SignatureTip:false, InfoTipRender:false, addEvents:false */

function Editor(textarea, connection, selfDebug, options) {
    const lineSeparator = '\r\n';
    const defaultLanguage = 'C#';
    const languageModes = {
        'C#': 'text/x-csharp',
        'Visual Basic': 'text/x-vb',
        'F#': 'text/x-fsharp',
        'PHP': 'application/x-httpd-php'
    };

    var language;
    var serverOptions;
    var lintingSuspended = true;
    var hadChangesSinceLastLinting = false;
    var capturedUpdateLinting;

    options = assign({ language: defaultLanguage }, options);
    options.on = assign({
        slowUpdateWait:   function() {},
        slowUpdateResult: function() {},
        textChange:       function() {},
        connectionChange: function() {},
        serverError:      function(message) { throw new Error(message); }
    }, options.on);

    const cmOptions = assign({ gutters: [], indentUnit: 4 }, options.forCodeMirror, {
        lineSeparator: lineSeparator,
        mode: languageModes[options.language],
        lint: { async: true, getAnnotations: lintGetAnnotations },
        lintFix: { getFixes: getFixes },
        infotip: { async: true, delay: 500, getInfo: infotipGetInfo, render: new InfoTipRender() }
    });
    cmOptions.gutters.push('CodeMirror-lint-markers');

    language = options.language;
    if (language !== defaultLanguage)
        serverOptions = { language: language };

    const cmSource = (function getCodeMirror() {
        const next = textarea.nextSibling;
        if (next && next.CodeMirror) {
            const existing = next.CodeMirror;
            for (var key in cmOptions) {
                existing.setOption(key, cmOptions[key]);
            }
            return { cm: existing, existing: true };
        }

        return { cm: CodeMirror.fromTextArea(textarea, cmOptions) };
    })();
    const cm = cmSource.cm;

    const keyMap = {
        'Shift-Tab': 'indentLess',
        'Ctrl-Space': function() { connection.sendCompletionState('force'); },
        'Shift-Ctrl-Space': function() { connection.sendSignatureHelpState('force'); },
        'Ctrl-.': 'lintFixShow',
        'Shift-Ctrl-Y': selfDebug ? function() { selfDebug.requestData(connection); } : null
    };
    cm.addKeyMap(keyMap);
    // see https://github.com/codemirror/CodeMirror/blob/dbaf6a94f1ae50d387fa77893cf6b886988c2147/addon/lint/lint.js#L133
    // ensures that next 'id' will be -1 whether a change happened or not
    cm.state.lint.waitingFor = -2;
    if (!cmSource.existing)
        setText(textarea.value);

    const getText = cm.getValue.bind(cm);
    if (selfDebug)
        selfDebug.watchEditor(getText, getCursorIndex);

    const cmWrapper = cm.getWrapperElement();
    cmWrapper.classList.add('mirrorsharp');
    cmWrapper.classList.add('mirrorsharp-theme');

    const hinter = new Hinter(cm, connection);
    const signatureTip = new SignatureTip(cm);
    const removeConnectionEvents = addEvents(connection, {
        open: function (e) {
            hideConnectionLoss();
            if (serverOptions)
                connection.sendSetOptions(serverOptions);

            const text = cm.getValue();
            if (text === '' || text == null) {
                lintingSuspended = false;
                return;
            }

            connection.sendReplaceText(0, 0, text, getCursorIndex(cm));
            options.on.connectionChange('open', e);
            lintingSuspended = false;
            hadChangesSinceLastLinting = true;
            if (capturedUpdateLinting)
                requestSlowUpdate();
        },
        message: onMessage,
        error: onCloseOrError,
        close: onCloseOrError
    });

    function onCloseOrError(e) {
        lintingSuspended = true;
        showConnectionLoss();
        options.on.connectionChange(e instanceof CloseEvent ? 'close' : 'error', e);
    }

    const indexKey = '$mirrorsharp-index';
    var changePending = false;
    var changeReason = null;
    var changesAreFromServer = false;
    const removeCMEvents = addEvents(cm, {
        beforeChange: function(s, change) {
            change.from[indexKey] = cm.indexFromPos(change.from);
            change.to[indexKey] = cm.indexFromPos(change.to);
            changePending = true;
        },
        cursorActivity: function () {
            if (changePending)
                return;
            connection.sendMoveCursor(getCursorIndex(cm));
        },
        changes: function(s, changes) {
            hadChangesSinceLastLinting = true;
            changePending = false;
            const cursorIndex = getCursorIndex(cm);
            changes = mergeChanges(changes);
            for (var i = 0; i < changes.length; i++) {
                const change = changes[i];
                const start = change.from[indexKey];
                const length = change.to[indexKey] - start;
                const text = change.text.join(lineSeparator);
                if (cursorIndex === start + 1 && text.length === 1 && !changesAreFromServer) {
                    if (length > 0)
                        connection.sendReplaceText(start, length, '', cursorIndex - 1);
                    connection.sendTypeChar(text);
                }
                else {
                    connection.sendReplaceText(start, length, text, cursorIndex, changeReason);
                }
            }
            options.on.textChange(getText);
        }
    });

    function mergeChanges(changes) {
        if (changes.length < 2)
            return changes;
        const results = [];
        var before = null;
        for (const change of changes) {
            if (changesCanBeMerged(before, change)) {
                before = {
                    from: before.from,
                    to: before.to,
                    text: [before.text[0] + change.text[0]],
                    origin: change.origin
                };
            }
            else {
                if (before)
                    results.push(before);
                before = change;
            }
        }
        results.push(before);
        return results;
    }

    function changesCanBeMerged(first, second) {
        return first && second
            && first.origin === 'undo'
            && second.origin === 'undo'
            && first.to.line === second.from.line
            && first.text.length === 1
            && second.text.length === 1
            && second.from.ch === second.to.ch
            && (first.to.ch + first.text[0].length) === second.from.ch;
    }

    function onMessage(message) {
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

            case 'signatures':
                signatureTip.update(message.signatures, message.span);
                break;

            case 'infotip':
                if (!message.entries) {
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
                selfDebug.displayData(message);
                break;

            case 'error':
                // [TEMP] Backward compatibility with 0.10
                if (/Unknown command: 'I'/.test(message.message)) {
                    cm.setOption('infotip', null);
                    return;
                }
                // [TEMP] end
                options.on.serverError(message.message);
                break;

            default:
                throw new Error('Unknown message type "' + message.type);
        }
    }

    function lintGetAnnotations(text, updateLinting) {
        if (!capturedUpdateLinting) {
            capturedUpdateLinting = function() {
                // see https://github.com/codemirror/CodeMirror/blob/dbaf6a94f1ae50d387fa77893cf6b886988c2147/addon/lint/lint.js#L133
                // ensures that next 'id' will always match 'waitingFor'
                cm.state.lint.waitingFor = -1;
                // eslint-disable-next-line no-invalid-this
                updateLinting.apply(this, arguments);
            };
        }
        requestSlowUpdate();
    }

    function getCursorIndex() {
        return cm.indexFromPos(cm.getCursor());
    }

    function setText(text) {
        cm.setValue(text.replace(/(\r\n|\r|\n)/g, '\r\n'));
    }

    function receiveServerChanges(changes, reason) {
        changesAreFromServer = true;
        changeReason = reason || 'server';
        for (var change of changes) {
            const from = cm.posFromIndex(change.start);
            const to = change.length > 0 ? cm.posFromIndex(change.start + change.length) : from;
            cm.replaceRange(change.text, from, to, '+server');
        }
        changeReason = null;
        changesAreFromServer = false;
    }

    // eslint-disable-next-line no-shadow
    function getFixes(cm, line, annotations) {
        var fixes = [];
        for (var i = 0; i < annotations.length; i++) {
            const diagnostic = annotations[i].diagnostic;
            if (!diagnostic.actions)
                continue;
            for (var j = 0; j < diagnostic.actions.length; j++) {
                var action = diagnostic.actions[j];
                fixes.push({
                    text: action.title,
                    apply: requestApplyFixAction,
                    id: action.id
                });
            }
        }
        return fixes;
    }

    // eslint-disable-next-line no-shadow
    function requestApplyFixAction(cm, line, fix) {
        connection.sendApplyDiagnosticAction(fix.id);
    }

    // eslint-disable-next-line no-shadow
    function infotipGetInfo(cm, position) {
        connection.sendRequestInfoTip(cm.indexFromPos(position));
    }

    function requestSlowUpdate(force) {
        if (lintingSuspended || !(hadChangesSinceLastLinting || force))
            return null;
        hadChangesSinceLastLinting = false;
        options.on.slowUpdateWait();
        return connection.sendSlowUpdate();
    }

    function showSlowUpdate(update) {
        const annotations = [];
        for (var diagnostic of update.diagnostics) {
            var severity = diagnostic.severity;
            if (diagnostic.severity === 'hidden') {
                if (diagnostic.tags.indexOf('unnecessary') < 0)
                    continue;

                severity = 'unnecessary';
            }

            var range = spanToRange(diagnostic.span);
            annotations.push({
                severity: severity,
                message: diagnostic.message,
                from: range.from,
                to: range.to,
                diagnostic: diagnostic
            });
        }
        capturedUpdateLinting(annotations);
        options.on.slowUpdateResult({
            diagnostics: update.diagnostics,
            x: update.x
        });
    }

    var connectionLossElement;
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

    function sendServerOptions(value) {
        return connection.sendSetOptions(value).then(function() {
            return requestSlowUpdate(true);
        });
    }

    function receiveServerOptions(value) {
        serverOptions = value;
        if (value.language !== language) {
            language = value.language;
            cm.setOption('mode', languageModes[language]);
        }
    }

    function spanToRange(span) {
        return {
            from: cm.posFromIndex(span.start),
            to: cm.posFromIndex(span.start + span.length)
        };
    }

    function destroy(destroyOptions) {
        cm.save();
        removeConnectionEvents();
        if (!destroyOptions.keepCodeMirror) {
            cm.toTextArea();
            return;
        }
        cm.removeKeyMap(keyMap);
        removeCMEvents();
        cm.setOption('lint', null);
        cm.setOption('lintFix', null);
    }

    this.getCodeMirror = function() { return cm; };
    this.setText = setText;
    this.getLanguage = function() { return language; };
    this.setLanguage = function(value) { return sendServerOptions({ language: value }); };
    this.sendServerOptions = sendServerOptions;
    this.destroy = destroy;
}