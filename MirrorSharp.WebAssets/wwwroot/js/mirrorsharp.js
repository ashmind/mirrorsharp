/* globals console:false */
(function (root, factory) {
    'use strict';
    if (typeof define === 'function' && define.amd) {
        define([
          'codemirror',
          'codemirror-addon-lint-fix',
          'codemirror/addon/hint/show-hint',
          'codemirror/mode/clike/clike',
          'codemirror/mode/vb/vb'
        ], factory);
    } else if (typeof module === 'object' && module.exports) {
        module.exports = factory(
          require('codemirror'),
          require('codemirror-addon-lint-fix'),
          require('codemirror/addon/hint/show-hint'),
          require('codemirror/mode/clike/clike'),
          require('codemirror/mode/vb/vb')
        );
    } else {
        root.mirrorsharp = factory(root.CodeMirror);
    }
}(this, function (CodeMirror) {
    'use strict';

    const assign = Object.assign || function (target) {
        for (var i = 1; i < arguments.length; i++) {
            var source = arguments[i];
            for (var key of source) {
                target[key] = source[key];
            }
        }
        return target;
    };

    function SelfDebug() {
        var getText;
        var getCursorIndex;
        const clientLog = [];
        var clientLogSnapshot;

        this.watchEditor = function(getTextValue, getCursorIndexValue) {
            getText = getTextValue;
            getCursorIndex = getCursorIndexValue;
        }

        this.log = function(event, message) {
            clientLog.push({
                time: new Date(),
                event: event,
                message: message,
                text: getText(),
                cursor: getCursorIndex()
            });
            while (clientLog.length > 100) {
                clientLog.shift();
            }
        };

        this.requestData = function(connection) {
            clientLogSnapshot = clientLog.slice(0);
            connection.sendRequestSelfDebugData();
        }

        this.displayData = function(serverData) {
            const log = [];
            for (var i = 0; i < clientLog.length; i++) {
                log.push({ entry: clientLog[i], on: 'client', index: i });
            }
            for (var i = 0; i < serverData.log.length; i++) {
                log.push({ entry: serverData.log[i], on: 'server', index: i });
            }
            log.sort(function(a, b) {
                if (a.on !== b.on) {
                    if (a.entry.time > b.entry.time) return +1;
                    if (a.entry.time < b.entry.time) return -1;
                    return 0;
                }
                if (a.index > b.index) return +1;
                if (a.index < b.index) return -1;
                return 0;
            });

            console.table(log.map(function(l) {
                var time = l.entry.time;
                var displayTime = ('0' + time.getHours()).slice(-2) + ':' + ('0' + time.getMinutes()).slice(-2) + ':' + ('0' + time.getSeconds()).slice(-2) + '.' + time.getMilliseconds();
                return {
                    time: displayTime,
                    message: l.entry.message,
                    event: l.on + ':' + l.entry.event,
                    cursor: l.entry.cursor,
                    text: l.entry.text
                };
            }));
        }
    }

    function Connection(openSocket, selfDebug) {
        var socket;
        var openPromise;
        const handlers = {
            open:    [],
            message: [],
            error:   [],
            close:   []
        };

        open();

        var reopenPeriod = 0;
        var reopenPeriodResetTimer;
        var reopening = false;
        function tryToReopen() {
            if (reopening)
                return;

            if (reopenPeriodResetTimer) {
                clearTimeout(reopenPeriodResetTimer);
                reopenPeriodResetTimer = null;
            }

            reopening = true;
            setTimeout(function () {
                open();
                reopening = false;
            }, reopenPeriod);
            if (reopenPeriod < 60000)
                reopenPeriod = Math.min(5 * (reopenPeriod + 200), 60000);
        }

        on('error', tryToReopen);
        on('close', tryToReopen);

        function open() {
            socket = openSocket();
            openPromise = new Promise(function (resolve) {
                socket.addEventListener('open', function () {
                    reopenPeriodResetTimer = setTimeout(function () { reopenPeriod = 0; }, reopenPeriod);
                    resolve();
                });
            });

            for (var key in handlers) {
                const keyFixed = key;
                const handlersByKey = handlers[key];
                /* jshint -W083 */
                socket.addEventListener(key, function (e) {
                    var argument = undefined;
                    if (keyFixed === 'message') {
                        argument = JSON.parse(e.data);
                        if (argument.type === 'self:debug') {
                            for (var entry of argument.log) {
                                entry.time = new Date(entry.time);
                            }
                        }
                        if (selfDebug)
                            selfDebug.log('before', JSON.stringify(argument));
                    }
                    for (var handler of handlersByKey) {
                        handler(argument);
                    }
                    if (selfDebug && keyFixed === 'message')
                        selfDebug.log('after', JSON.stringify(argument));
                });
                /* jshint +W083 */
            }
        }

        function on(key, handler) {
            handlers[key].push(handler);
        }

        function sendWhenOpen(command) {
            return openPromise.then(function () {
                if (selfDebug)
                    selfDebug.log('send', command);
                socket.send(command);
            });
        }

        this.on = on;
        this.sendReplaceText = function (start, length, newText, cursorIndexAfter, reason) {
            return sendWhenOpen('R' + start + ':' + length + ':' + cursorIndexAfter + ':' + (reason || '') + ':' + newText);
        };

        this.sendMoveCursor = function(cursorIndex) {
            return sendWhenOpen('M' + cursorIndex);
        };

        this.sendTypeChar = function(char) {
            return sendWhenOpen('C' + char);
        };

        const completionCommandMap = { cancel: 'X', force: 'F' };
        this.sendCompletionState = function(indexOrCommand) {
            const argument = completionCommandMap[indexOrCommand] || indexOrCommand;
            return sendWhenOpen('S' + argument);
        };

        this.sendSlowUpdate = function() {
            return sendWhenOpen('U');
        };

        this.sendApplyDiagnosticAction = function(actionId) {
            return sendWhenOpen('F' + actionId);
        };

        this.sendSetOptions = function(options) {
            const optionPairs = [];
            for (var key in options) {
                optionPairs.push(key + "=" + options[key]);
            }
            return sendWhenOpen('O' + optionPairs.join(','));
        };

        this.sendRequestSelfDebugData = function() {
            return sendWhenOpen('Y');
        }
    }

    function Hinter(cm, connection) {
        const indexInListKey = '$mirrorsharp-indexInList';
        const priorityKey = '$mirrorsharp-priority';
        var state = 'stopped';
        var hasSuggestion;
        var currentOptions;
        var lastCommitChar;

        const commit = function (cm, data, item) {
            connection.sendCompletionState(item[indexInListKey]);
            state = 'committed';
        };

        const cancel = function(cm) {
            if (cm.state.completionActive)
                cm.state.completionActive.close();
        }

        this.start = function(list, span, options) {
            state = 'starting';
            currentOptions = options;
            lastCommitChar = null;
            const hintStart = cm.posFromIndex(span.start);
            const hintList = list.map(function (c, index) {
                const item = {
                    text: c.filterText,
                    displayText: c.displayText,
                    className: 'mirrorsharp-hint ' + c.tags.map(function (t) { return 'mirrorsharp-hint-' + t.toLowerCase(); }).join(' '),
                    hint: commit
                };
                item[indexInListKey] = index;
                item[priorityKey] = c.priority;
                if (c.span)
                    item.from = cm.posFromIndex(c.span.start);
                return item;
            });
            const suggestion = options.suggestion;
            hasSuggestion = !!suggestion;
            if (hasSuggestion) {
                hintList.unshift({
                    displayText: suggestion.displayText,
                    className: 'mirrorsharp-hint mirrorsharp-hint-suggestion',
                    hint: cancel
                });
            }
            cm.showHint({
                hint: function() {
                    const prefix = cm.getRange(hintStart, cm.getCursor());
                    var list = hintList;
                    if (prefix.length > 0) {
                        var regexp = new RegExp('^' + prefix.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&'), 'i');
                        list = hintList.filter(function(item, index) {
                            return (hasSuggestion && index === 0) || regexp.test(item.text);
                        });
                        if (hasSuggestion && list.length === 1)
                            list = [];
                    }
                    if (!hasSuggestion) {
                        // does not seem like I can use selectedHint here, as it does not force the scroll
                        var selectedIndex = indexOfItemWithMaxPriority(list);
                        if (selectedIndex > 0)
                            setTimeout(function() { cm.state.completionActive.widget.changeActive(selectedIndex); }, 0);
                    }

                    return { from: hintStart, list: list };
                },
                completeSingle: false
            });
            state = 'started';
        };

        cm.on('keypress', function(_, e) {
            if (state === 'stopped')
                return;
            const key = e.key || String.fromCharCode(e.charCode || e.keyCode);
            if (currentOptions.commitChars.indexOf(key) > -1) {
                const widget = cm.state.completionActive.widget;
                if (!widget) {
                    cancel();
                    return;
                }
                widget.pick();
            }
        });

        cm.on('endCompletion', function() {
            if (state === 'starting')
                return;
            if (state === 'started')
                connection.sendCompletionState('cancel');
            state = 'stopped';
        });

        function indexOfItemWithMaxPriority(list) {
            var maxPriority = 0;
            var result = 0;
            for (var i = 0; i < list.length; i++) {
                const priority = list[i][priorityKey];
                if (priority > maxPriority) {
                    result = i;
                    maxPriority = priority;
                }
            }
            return result;
        }
    }

    function SignatureTip(cm) {
        const displayKindToClassMap = {
            keyword: 'cm-keyword'
        };

        var active = false;
        var tooltip;
        var ol;

        const hide = function() {
            if (!active)
                return;

            document.body.removeChild(tooltip);
            active = false;
        };

        this.update = function(signatures, span) {
            if (!tooltip) {
                tooltip = document.createElement('div');
                tooltip.className = 'mirrorsharp-theme mirrorsharp-signature-tooltip';
                ol = document.createElement('ol');
                tooltip.appendChild(ol);
            }

            if (!signatures || signatures.length === 0) {
                if (active)
                    hide();
                return;
            }

            while (ol.firstChild) {
                ol.removeChild(ol.firstChild);
            }
            for (var signature of signatures) {
                var li = document.createElement('li');
                if (signature.selected)
                    li.className = 'mirrorsharp-signature-selected';

                for (var part of signature.parts) {
                    var className = displayKindToClassMap[part.kind] || '';
                    if (part.selected)
                        className += ' mirrorsharp-signature-part-selected';

                    var child;
                    if (className) {
                        child = document.createElement('span');
                        child.className = className;
                        child.textContent = part.text;
                    }
                    else {
                        child = document.createTextNode(part.text);
                    }
                    li.appendChild(child);
                }
                ol.appendChild(li);
            }

            const startPos = cm.posFromIndex(span.start);

            active = true;

            const startCharCoords = cm.charCoords(startPos);
            tooltip.style.top = startCharCoords.bottom + 'px';
            tooltip.style.left = startCharCoords.left + 'px';
            document.body.appendChild(tooltip);
        };

        this.hide = hide;
    }

    function Editor(textarea, connection, selfDebug, options) {
        const lineSeparator = '\r\n';
        var serverOptions;
        var lintingSuspended = true;
        var capturedUpdateLinting;

        options = assign({}, {
            forCodeMirror: {},
            afterSlowUpdate: function() {},
            afterTextChange: function() {},
            onServerError: function(message) { throw new Error(message); }
        }, options);
        const cmOptions = assign({ gutters: [], indentUnit: 4 }, options.forCodeMirror, {
            lineSeparator: lineSeparator,
            mode: 'text/x-csharp',
            lint: { async: true, getAnnotations: lintGetAnnotations },
            lintFix: { getFixes: getFixes },
            extraKeys: {}
        });
        cmOptions.extraKeys = assign({
            'Ctrl-Space': function() { connection.sendCompletionState('force'); },
            'Ctrl-.': 'lintFixShow',
            'Shift-Ctrl-Y': selfDebug ? function() { selfDebug.requestData(connection); } : null
        }, cmOptions.extraKeys);

        cmOptions.gutters.push('CodeMirror-lint-markers');

        const cm = CodeMirror.fromTextArea(textarea, cmOptions);
        // see https://github.com/codemirror/CodeMirror/blob/dbaf6a94f1ae50d387fa77893cf6b886988c2147/addon/lint/lint.js#L133
        // ensures that next 'id' will be -1 whther a change happened or not
        cm.state.lint.waitingFor = -2;
        cm.setValue(textarea.value.replace(/(\r\n|\r|\n)/g, '\r\n'));

        const getText = cm.getValue.bind(cm);
        if (selfDebug)
            selfDebug.watchEditor(getText, getCursorIndex);

        const cmWrapper = cm.getWrapperElement();
        cmWrapper.classList.add('mirrorsharp');
        cmWrapper.classList.add('mirrorsharp-theme');

        const hinter = new Hinter(cm, connection);
        const signatureTip = new SignatureTip(cm);
        connection.on('open', function () {
            hideConnectionLoss();
            if (serverOptions)
                connection.sendSetOptions(serverOptions);

            const text = cm.getValue();
            if (text === '' || text == null) {
                lintingSuspended = false;
                return;
            }

            connection.sendReplaceText(0, 0, text, getCursorIndex(cm));
            lintingSuspended = false;
            if (capturedUpdateLinting)
                requestSlowUpdate();
        });

        function onCloseOrError() {
            lintingSuspended = true;
            showConnectionLoss();
        }

        connection.on('error', onCloseOrError);
        connection.on('close', onCloseOrError);

        const indexKey = '$mirrorsharp-index';
        var changePending = false;
        var changeReason = null;
        var changesAreFromServer = false;
        cm.on('beforeChange', function (s, change) {
            change.from[indexKey] = cm.indexFromPos(change.from);
            change.to[indexKey] = cm.indexFromPos(change.to);
            changePending = true;
        });

        cm.on('cursorActivity', function () {
            if (changePending)
                return;
            connection.sendMoveCursor(getCursorIndex(cm));
        });

        cm.on('changes', function (s, changes) {
            const cursorIndex = getCursorIndex(cm);
            changePending = false;
            if (changesAreFromServer && changeReason === 'fix' /*TODO, gh-38*/) {
                connection.sendMoveCursor(cursorIndex);
                options.afterTextChange(getText);
                return;
            }

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
            options.afterTextChange(getText);
        });

        connection.on('message', function (message) {
            switch (message.type) {
                case 'changes':
                    applyServerChanges(message.changes, message.reason);
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

                case 'slowUpdate':
                    showSlowUpdate(message);
                    break;

                case 'optionsEcho':
                    serverOptions = message.options;
                    break;

                case 'self:debug':
                    selfDebug.displayData(message);
                    break;

                case 'error':
                    options.onServerError(message.message);
                    break;

                default:
                    throw new Error('Unknown message type "' + message.type);
            }
        });

        function lintGetAnnotations(text, updateLinting) {
            if (!capturedUpdateLinting) {
                capturedUpdateLinting = function() {
                    // see https://github.com/codemirror/CodeMirror/blob/dbaf6a94f1ae50d387fa77893cf6b886988c2147/addon/lint/lint.js#L133
                    // ensures that next 'id' will always match 'waitingFor'
                    cm.state.lint.waitingFor = -1;
                    updateLinting.apply(this, arguments);
                };
            }
            requestSlowUpdate(text);
        }

        function getCursorIndex() {
            return cm.indexFromPos(cm.getCursor());
        }
        
        function applyServerChanges(changes, reason) {
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

        function requestApplyFixAction(cm, line, fix) {
            connection.sendApplyDiagnosticAction(fix.id);
        }

        function requestSlowUpdate() {
            if (lintingSuspended)
                return null;
            return connection.sendSlowUpdate();
        }

        function showSlowUpdate(update) {
            const annotations = [];
            for (var diagnostic of update.diagnostics) {
                var severity = diagnostic.severity;
                if (diagnostic.severity === 'hidden') {
                    if (diagnostic.tags.indexOf('unnecessary') === 0)
                        continue;

                    severity = 'unnecessary';
                }

                annotations.push({
                    severity: severity,
                    message: diagnostic.message,
                    from: cm.posFromIndex(diagnostic.span.start),
                    to: cm.posFromIndex(diagnostic.span.start + diagnostic.span.length),
                    diagnostic: diagnostic
                });
            }
            capturedUpdateLinting(annotations);
            options.afterSlowUpdate({
                diagnostics: update.diagnostics,
                x: update.x
            });
        }

        var connectionLossElement;
        function showConnectionLoss() {
            const wrapper = cm.getWrapperElement();
            if (!connectionLossElement) {
                connectionLossElement = document.createElement("div");
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
                return requestSlowUpdate();
            });
        }

        this.sendServerOptions = sendServerOptions;
    }

    return function(textarea, options) {
        const selfDebug = options.selfDebugEnabled ? new SelfDebug() : null;
        const connection = new Connection(function() {
            return new WebSocket(options.serviceUrl);
        }, selfDebug);
        const editor = new Editor(textarea, connection, selfDebug, options);
        return {
            sendServerOptions: editor.sendServerOptions.bind(editor)
        };
    };
}));