/* globals define:false */
(function (root, factory) {
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
            close:   []
        };

        open();
        on('close', function() {
            setTimeout(function() { open(); }, 1000);
        });

        function open() {
            socket = openSocket();
            openPromise = new Promise(function (resolve) {
                socket.addEventListener('open', function() {
                    resolve();
                });
            });

            for (var key in handlers) {
                const keyFixed = key;
                const handlersByKey = handlers[key];
                socket.addEventListener(key, function (e) {
                    var argument = (keyFixed === 'message') ? JSON.parse(e.data) : undefined;
                    for (var handler of handlersByKey) {
                        handler(argument);
                    }
                });
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
        }

        this.sendMoveCursor = function(cursorIndex) {
            return sendWhenOpen('C' + cursorIndex);
        }

        this.sendTypeChar = function (char) {
            return sendWhenOpen('T' + char);
        }

        this.sendCommitCompletion = function (itemIndex) {
            return sendWhenOpen('S' + itemIndex);
        }

        this.sendGetDiagnostics = function () {
            return sendWhenOpen('D');
        }
    }

    function Editor(textarea, connection, options) {
        const cmOptions = options.forCodeMirror || { mode: 'text/x-csharp', gutters: [] };
        cmOptions.lint = { async: true, getAnnotations: requestDiagnostics };
        cmOptions.gutters.push('CodeMirror-lint-markers');
        const cm = CodeMirror.fromTextArea(textarea, cmOptions);

        /*(function createStatusElement() {
            const cmWrapper = cm.getWrapperElement();
            const element = document.createElement('div');
            element.className = 'mirrorsharp-status';
            cmWrapper.appendChild(element);
            connection.onOpen(function () {
                element.classList.add('mirrorsharp-status-connected');
            });

            return element;
        })();*/

        var lintingSuspended = true;
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
                requestDiagnostics(text, updateLinting);
        });

        connection.on('close', function () {
            lintingSuspended = true;
            showConnectionLoss();
        });

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
            for (var i = 0; i < changes.length; i++) {
                const change = changes[i];
                const start = change.from[indexKey];
                const length = change.to[indexKey] - start;
                const text = change.text.join('\n');
                if (cursorIndex === start + 1 && text.length === 1 && !changesAreFromServer) {
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
                    showCompletions(message.completions);
                    break;

                case 'diagnostics':
                    showDiagnostics(message.diagnostics);
                    break;

                case 'debug:compare':
                    debugCompare(message.text, message.cursor);
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
                cm.replaceRange(change.text, from, to);
            }
            changesAreFromServer = false;
        }

        function showCompletions(completions) {
            const indexInListKey = '$mirrorsharp-indexInList';
            var commit = function (cm, data, item) {
                connection.sendCommitCompletion(item[indexInListKey]);
            }

            var hintResult = {
                from: cm.posFromIndex(completions.span.start),
                list: completions.list.map(function (c, index) {
                    const item = {
                        displayText: c.displayText,
                        className: 'mirrorsharp-hint ' + c.tags.map(function (t) { return 'mirrorsharp-hint-' + t.toLowerCase(); }).join(' '),
                        hint: commit
                    };
                    item[indexInListKey] = index;
                    if (c.span)
                        item.from = cm.posFromIndex(c.span.start);
                    return item;
                })
            }
            cm.showHint({
                hint: function () { return hintResult; },
                completeSingle: false
            });
        }

        function requestDiagnostics(text, updateLintingValue) {
            updateLinting = updateLintingValue;
            if (!lintingSuspended)
                connection.sendGetDiagnostics();
        }

        function showDiagnostics(diagnostics) {
            const annotations = diagnostics.map(function(d) {
                return {
                    severity: d.severity,
                    message: d.message,
                    from: cm.posFromIndex(d.span.start),
                    to: cm.posFromIndex(d.span.start + d.span.length)
                }
            });
            updateLinting(annotations);
        }

        function debugCompare(serverText, serverCursorIndex) {
            if (serverText !== undefined) {
                const clientText = cm.getValue();
                if (clientText !== serverText)
                    console.error('Client text does not match server text:', { clientText: clientText, serverText: serverText });
            }

            const clientCursorIndex = getCursorIndex();
            if (clientCursorIndex !== serverCursorIndex)
                console.error('Client cursor position does not match server position:', { clientPosition: clientCursorIndex, serverPosition: serverCursorIndex });
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
    }
}));