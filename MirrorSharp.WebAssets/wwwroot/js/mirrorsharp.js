/* globals console:false */
(function (root, factory) {
    'use strict';
    if (typeof define === 'function' && define.amd) {
        define(['CodeMirror'], factory);
    } else if (typeof module === 'object' && module.exports) {
        module.exports = factory(require('CodeMirror'));
    } else {
        root.mirrorsharp = factory(root.CodeMirror);
    }
}(this, function (CodeMirror) {
    'use strict';

    function Connection(openSocket) {
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
                    var argument = (keyFixed === 'message') ? JSON.parse(e.data) : undefined;
                    for (var handler of handlersByKey) {
                        handler(argument);
                    }
                });
                /* jshint +W083 */
            }
        }

        function on(key, handler) {
            handlers[key].push(handler);
        }

        function sendWhenOpen(command) {
            openPromise.then(function () {
                //console.debug("[=>]", command);
                socket.send(command);
            });
        }

        this.on = on;

        this.sendReplaceText = function (isLastOrOnly, start, length, newText, cursorIndexAfter) {
            const command = isLastOrOnly ? 'R' : 'P';
            return sendWhenOpen(command + start + ':' + length + ':' + cursorIndexAfter + ':' + newText);
        };

        this.sendMoveCursor = function(cursorIndex) {
            return sendWhenOpen('M' + cursorIndex);
        };

        this.sendTypeChar = function(char) {
            return sendWhenOpen('C' + char);
        };

        this.sendCompletionChoice = function(index) {
            return sendWhenOpen('S' + (index != null ? index : 'X'));
        };

        this.sendSlowUpdate = function() {
            return sendWhenOpen('U');
        };

        this.sendApplyDiagnosticAction = function(actionId) {
            return sendWhenOpen('F' + actionId);
        };
    }

    function Hinter(cm, connection) {
        const state = this;
        const indexInListKey = '$mirrorsharp-indexInList';

        var committed = false;
        const commit = function (cm, data, item) {
            connection.sendCompletionChoice(item[indexInListKey]);
            committed = true;
        };

        state.active = false;
        this.start = function(list, span) {
            state.active = true;
            committed = false;
            const hintStart = cm.posFromIndex(span.start);
            const hintList = list.map(function (c, index) {
                const item = {
                    text: c.filterText,
                    displayText: c.displayText,
                    className: 'mirrorsharp-hint ' + c.tags.map(function (t) { return 'mirrorsharp-hint-' + t.toLowerCase(); }).join(' '),
                    hint: commit
                };
                item[indexInListKey] = index;
                if (c.span)
                    item.from = cm.posFromIndex(c.span.start);
                return item;
            });
            cm.showHint({
                hint: function() {
                    const prefix = cm.getRange(hintStart, cm.getCursor());
                    var list = hintList;
                    if (prefix.length > 0)
                        list = hintList.filter(function(item) { return item.text.indexOf(prefix) === 0; });

                    return { from: hintStart, list: list };
                },
                completeSingle: false
            });
        };

        cm.on('endCompletion', function() {
            state.active = false;
            if (committed)
                return;
            connection.sendCompletionChoice(null);
        });
    }

    function SignatureTip(cm) {
        const displayKindToClassMap = {
            keyword: 'cm-keyword'
        };

        var tooltip;
        var ol;
        this.show = function(signatures, span) {
            if (!tooltip) {
                tooltip = document.createElement('div');
                tooltip.className = 'mirrorsharp-theme mirrorsharp-signatures-tooltip';
                ol = document.createElement('ol');
                tooltip.appendChild(ol);
            }
            while (ol.firstChild) {
                ol.removeChild(ol.firstChild);
            }
            for (var signature of signatures) {
                var li = document.createElement('li');
                for (var part of signature) {
                    const className = displayKindToClassMap[part.kind];
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

            const startCharCoords = cm.charCoords(cm.posFromIndex(span.start));
            tooltip.style.top = startCharCoords.bottom + 'px';
            tooltip.style.left = startCharCoords.left + 'px';
            document.body.appendChild(tooltip);
        };
    }

    function Editor(textarea, connection, options) {
        const lineSeparator = '\r\n';
        var lintingSuspended = true;

        const cmOptions = options.forCodeMirror || {
            lineSeparator: lineSeparator,
            mode: 'text/x-csharp',
            gutters: []
        };
        cmOptions.lint = { async: true, getAnnotations: requestSlowUpdate };
        cmOptions.lintFix = { getFixes: getFixes };
        cmOptions.gutters.push('CodeMirror-lint-markers');
        const cm = CodeMirror.fromTextArea(textarea, cmOptions);
        cm.setValue(textarea.value.replace(/(\r\n|\r|\n)/g, '\r\n'));

        const cmWrapper = cm.getWrapperElement();
        cmWrapper.classList.add('mirrorsharp');
        cmWrapper.classList.add('mirrorsharp-theme');

        const hinter = new Hinter(cm, connection);
        const signatureTip = new SignatureTip(cm);

        var updateLinting;
        connection.on('open', function () {
            hideConnectionLoss();

            const text = cm.getValue();
            if (text === '' || text == null) {
                lintingSuspended = false;
                return;
            }

            connection.sendReplaceText(true, 0, 0, text, getCursorIndex(cm));
            lintingSuspended = false;
            if (updateLinting)
                requestSlowUpdate(text, updateLinting);
        });

        function onCloseOrError() {
            lintingSuspended = true;
            showConnectionLoss();
        }

        connection.on('error', onCloseOrError);
        connection.on('close', onCloseOrError);

        const indexKey = '$mirrorsharp-index';
        var changePending = false;
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
            if (changesAreFromServer) {
                connection.sendMoveCursor(cursorIndex);
                return;
            }

            for (var i = 0; i < changes.length; i++) {
                const change = changes[i];
                const start = change.from[indexKey];
                const length = change.to[indexKey] - start;
                const text = change.text.join(lineSeparator);
                if (cursorIndex === start + 1 && length === 0 && text.length === 1 && !changesAreFromServer) {
                    connection.sendTypeChar(text);
                }
                else {
                    const lastOrOnly = (i === changes.length - 1);
                    connection.sendReplaceText(lastOrOnly, start, length, text, cursorIndex);
                }
            }
        });

        connection.on('message', function (message) {
            switch (message.type) {
                case 'changes':
                    applyChangesFromServer(message.changes);
                    break;

                case 'completions':
                    hinter.start(message.completions, message.span);
                    break;

                case 'signatures':
                    signatureTip.show(message.signatures, message.span);
                    break;

                case 'slowUpdate':
                    showSlowUpdate(message);
                    break;

                case 'debug:compare':
                    debugCompare(message);
                    break;

                case 'error':
                    throw new Error(message.message);

                default:
                    throw new Error('Unknown message type "' + message.type);
            }
        });

        function getCursorIndex() {
            return cm.indexFromPos(cm.getCursor());
        }

        function applyChangesFromServer(changes) {
            changesAreFromServer = true;
            for (var change of changes) {
                const from = cm.posFromIndex(change.start);
                const to = change.length > 0 ? cm.posFromIndex(change.start + change.length) : from;
                cm.replaceRange(change.text, from, to, '+server');
            }
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

        function requestSlowUpdate(text, updateLintingValue) {
            updateLinting = updateLintingValue;
            if (!lintingSuspended)
                connection.sendSlowUpdate();
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
            updateLinting(annotations);
        }

        function debugCompare(server) {
            if (server.text !== undefined) {
                const clientText = cm.getValue();
                if (clientText !== server.text)
                    console.error('Client text does not match server text:', { clientText: clientText, serverText: server.text });
            }

            const clientCursorIndex = getCursorIndex();
            if (clientCursorIndex !== server.cursor)
                console.error('Client cursor position does not match server position:', { clientPosition: clientCursorIndex, serverPosition: server.cursor });

            if (hinter.active !== server.completion)
                console.error('Client completion state does not match server state:', { clientCompletionActive: hinter.active, serverCompletionActive: server.completion });
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
    }

    return function(textarea, options) {
        const connection = new Connection(function() {
            return new WebSocket(options.serviceUrl);
        });

        return new Editor(textarea, connection, options);
    };
}));