(function (root, factory) {
    if (typeof define === 'function' && define.amd) {
        define(['CodeMirror'], factory);
    } else if (typeof module === 'object' && module.exports) {
        module.exports = factory(require('CodeMirror'));
    } else {
        root.mirrorsharp = factory(root.CodeMirror);
    }
}(this, function (CodeMirror) {
    function Connection(socket) {
        const openPromise = new Promise(function(resolve) {
            socket.addEventListener('open', function (e) {
                //console.debug("[open]");
                resolve();
            });
        });

        function sendWhenOpen(command) {
            openPromise.then(function () {
                //console.debug("[=>]", command);
                socket.send(command);
            });
        }

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

        this.onMessage = function(handler) {
            socket.addEventListener('message', function (e) {
                //console.debug("[<=]", e.data);
                const message = JSON.parse(e.data);
                handler(message);
            });
        }
    }

    function Editor(textarea, connection, options) {
        const cmOptions = options.forCodeMirror || { mode: 'text/x-csharp', gutters: [] };
        cmOptions.lint = { async: true, getAnnotations: requestDiagnostics };
        cmOptions.gutters.push('CodeMirror-lint-markers');
        const cm = CodeMirror.fromTextArea(textarea, cmOptions);

        var initialTextSent = false;
        var updateLinting;
        (function sendOnStart() {
            const text = cm.getValue();
            if (text === '' || text == null) {
                initialTextSent = true;
                return;
            }

            connection.sendReplaceText(true, 0, 0, text, 0);
            initialTextSent = true;
            if (updateLinting)
                requestDiagnostics(text, updateLinting);
        })();

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

        connection.onMessage(function (message) {
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
            if (initialTextSent)
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
    }

    return function(textarea, options) {
        const connection = new Connection(new WebSocket(options.serviceUrl));
        return new Editor(textarea, connection, options);
    }
}));