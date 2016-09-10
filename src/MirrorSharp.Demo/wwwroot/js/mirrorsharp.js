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

    return function(textarea, options) {
        const socket = new WebSocket(options.serviceUrl);

        const cmOptions = options.forCodeMirror || { gutters: [] };
        //cmOptions.lint = { async: true, getAnnotations: lint };
        cmOptions.gutters.push('CodeMirror-lint-markers');

        const cm = CodeMirror.fromTextArea(textarea, cmOptions);
        const indexKey = '$$mirrorsharp_index$$';

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
            socket.send('C' + cursorIndex);
        });

        cm.on('changes', function(s, changes) {
            const cursorIndex = getCursorIndex(cm);
            changePending = false;
            for (var change of changes) {
                const start = change.from[indexKey];
                const length = change.to[indexKey] - start;
                const text = change.text;
                var message;
                if (cursorIndex === start + 1 && text.length === 1) {
                    // typed a character
                    message = 'T' + text;
                }
                else {
                    // everything else
                    message = 'R' + start + ':' + length + ':' + cursorIndex + ':' + text;
                }

                socket.send(message);
            }
        });

        socket.addEventListener('message', function (e) {
            const message = JSON.parse(e.data);
            switch (message.type) {
                case 'completions':
                    showCompletions(cm, message.completions);
                    break;
            }
        });
    }
}));