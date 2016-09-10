(function (root, factory) {
    if (typeof define === 'function' && define.amd) {
        define(['CodeMirror'], factory);
    } else if (typeof module === 'object' && module.exports) {
        module.exports = factory(require('CodeMirror'));
    } else {
        root.mirrorsharp = factory(root.CodeMirror);
    }
}(this, function (CodeMirror) {
    function getCursorIndex(cm) {
        return cm.indexFromPos(cm.getCursor());
    }

    function showCompletions(cm, completions) {
        cm.showHint({
            hint: function () {
                return { list: completions };
            },
            completeSingle: false
        });
    }

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

        this.onMessage = function(handler) {
            socket.addEventListener('message', function (e) {
                const message = JSON.parse(e.data);
                handler(message);
            });
        }
    }

    return function(textarea, options) {
        const connection = new Connection(new WebSocket(options.serviceUrl));

        const cmOptions = options.forCodeMirror || { gutters: [] };
        //cmOptions.lint = { async: true, getAnnotations: lint };
        cmOptions.gutters.push('CodeMirror-lint-markers');

        const cm = CodeMirror.fromTextArea(textarea, cmOptions);
        const indexKey = '$$mirrorsharp_index$$';

        (function() {
            const value = cm.getValue();
            if (value !== '' && value != null)
                connection.sendReplaceText(0, 0, value, 0);
        })();

        var changePending = false;
        cm.on('beforeChange', function(s, change) {
            change.from[indexKey] = cm.indexFromPos(change.from);
            change.to[indexKey] = cm.indexFromPos(change.to);
            changePending = true;
        });

        cm.on('cursorActivity', function() {
            if (changePending)
                return;
            const cursorIndex = getCursorIndex(cm);
            connection.sendMoveCursor(cursorIndex);
        });

        cm.on('changes', function(s, changes) {
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
                case 'completions':
                    showCompletions(cm, message.completions);
                    break;
            }
        });
    }
}));