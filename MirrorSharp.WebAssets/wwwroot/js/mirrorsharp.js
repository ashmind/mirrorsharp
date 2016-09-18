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
                    reopenPeriodReset = setTimeout(function () { reopenPeriod = 0; }, reopenPeriod);
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
            return sendWhenOpen('M' + cursorIndex);
        }

        this.sendTypeChar = function (char) {
            return sendWhenOpen('C' + char);
        }

        this.sendCommitCompletion = function (itemIndex) {
            return sendWhenOpen('S' + itemIndex);
        }

        this.sendSlowUpdate = function () {
            return sendWhenOpen('U');
        }
    }

    function Editor(textarea, connection, options) {
        var lintingSuspended = true;

        const cmOptions = options.forCodeMirror || { mode: 'text/x-csharp', gutters: [] };
        cmOptions.lint = { async: true, getAnnotations: requestSlowUpdate };
        cmOptions.gutters.push('CodeMirror-lint-markers');
        const cm = CodeMirror.fromTextArea(textarea, cmOptions);

        cm.getWrapperElement().classList.add('mirrorsharp');

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
            for (var i = 0; i < changes.length; i++) {
                const change = changes[i];
                const start = change.from[indexKey];
                const length = change.to[indexKey] - start;
                const text = change.text.join('\n');
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
                    showCompletions(message.completions);
                    break;

                case 'slowUpdate':
                    showSlowUpdate(message);
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

        function requestSlowUpdate(text, updateLintingValue) {
            updateLinting = updateLintingValue;
            if (!lintingSuspended)
                connection.sendSlowUpdate();
        }

        var markers = [];
        const markerOptions = { className: 'mirrorsharp-marker-unnecessary' };
        function showSlowUpdate(update) {
            const annotations = [];
            for (var marker of markers) {
                marker.clear();
            }
            markers = [];

            for (var diagnostic of update.diagnostics) {
                if (diagnostic.severity === 'hidden' && diagnostic.tags.indexOf('unnecessary') >= 0) {
                    markers.push(cm.markText(
                        cm.posFromIndex(diagnostic.span.start),
                        cm.posFromIndex(diagnostic.span.start + diagnostic.span.length),
                        markerOptions
                    ));
                }

                if (diagnostic.severity !== 'error' && diagnostic.severity !== 'warning')
                    continue;

                annotations.push({
                    severity: diagnostic.severity,
                    message: diagnostic.message,
                    from: cm.posFromIndex(diagnostic.span.start),
                    to: cm.posFromIndex(diagnostic.span.start + diagnostic.span.length)
                });
            }
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