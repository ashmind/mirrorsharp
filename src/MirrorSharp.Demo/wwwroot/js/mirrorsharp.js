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
                resolve();
            });
        });

        function sendWhenOpen(command) {
            openPromise.then(function() { socket.send(command); });
        }

        this.sendReplaceText = function (start, length, newText, cursorIndexAfter) {
            return sendWhenOpen('R' + start + ':' + length + ':' + cursorIndexAfter + ':' + newText);
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

        this.onMessage = function(handler) {
            socket.addEventListener('message', function (e) {
                const message = JSON.parse(e.data);
                handler(message);
            });
        }
    }

    function getCursorIndex(cm) {
        return cm.indexFromPos(cm.getCursor());
    }

    function showCompletions(cm, completions, connection) {
        const indexInListKey = '$mirrorsharp-indexInList';
        var commit = function(cm, data, item) {
            connection.sendCommitCompletion(item[indexInListKey]);
        }

        var hintResult = {
            from: cm.posFromIndex(completions.span.start),
            list: completions.list.map(function (c, index) {
                const item = {
                    displayText: c.displayText,
                    className: c.tags.map(function (t) { return 'mirrorsharp-hint-' + t.toLowerCase(); }).join(' '),
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

    return function (textarea, options) {
        const connection = new Connection(new WebSocket(options.serviceUrl));

        const cmOptions = options.forCodeMirror || { gutters: [] };
        //cmOptions.lint = { async: true, getAnnotations: lint };
        cmOptions.gutters.push('CodeMirror-lint-markers');

        const cm = CodeMirror.fromTextArea(textarea, cmOptions);

        var resetting = false;
        function reset(newText, newCursorIndex) {
            resetting = true;
            if (newCursorIndex == null)
                newCursorIndex = cm.indexFromPos(cm.getCursor());

            cm.setValue(newText);
            cm.setCursor(cm.posFromIndex(newCursorIndex));
            resetting = false;
        }

        (function() {
            const value = cm.getValue();
            if (value !== '' && value != null)
                connection.sendReplaceText(0, 0, value, 0);
        })();

        const indexKey = '$mirrorsharp-index';
        var changePending = false;
        cm.on('beforeChange', function (s, change) {
            if (resetting)
                return;

            change.from[indexKey] = cm.indexFromPos(change.from);
            change.to[indexKey] = cm.indexFromPos(change.to);
            changePending = true;
        });

        cm.on('cursorActivity', function() {
            if (resetting || changePending)
                return;
            const cursorIndex = getCursorIndex(cm);
            connection.sendMoveCursor(cursorIndex);
        });

        cm.on('changes', function (s, changes) {
            if (resetting)
                return;

            const cursorIndex = getCursorIndex(cm);
            changePending = false;
            for (var change of changes) {
                const start = change.from[indexKey];
                const length = change.to[indexKey] - start;
                const text = change.text;
                if (cursorIndex === start + 1 && text.length === 1) {
                    connection.sendTypeChar(text);
                }
                else {
                    connection.sendReplaceText(start, length, text, cursorIndex);
                }
            }
        });

        connection.onMessage(function (message) {
            switch (message.type) {
                case 'reset':
                    reset(message.text, message.cursor);
                    break;

                case 'completions':
                    showCompletions(cm, message.completions, connection);
                    break;
            }
        });
    }
}));