/* globals addEvents:false */

function Connection(url, selfDebug) {
    var socket;
    var openPromise;
    const handlers = {
        open:    [],
        message: [],
        error:   [],
        close:   []
    };

    open();

    var mustBeClosed = false;
    var reopenPeriod = 0;
    var reopenPeriodResetTimer;
    var reopening = false;

    function open() {
        socket = new WebSocket(url);
        openPromise = new Promise(function (resolve) {
            socket.addEventListener('open', function () {
                reopenPeriodResetTimer = setTimeout(function () { reopenPeriod = 0; }, reopenPeriod);
                resolve();
            });
        });

        for (var key in handlers) {
            const keyFixed = key;
            const handlersByKey = handlers[key];
            socket.addEventListener(key, function (e) {
                const handlerArguments = [e];
                if (keyFixed === 'message') {
                    const data = JSON.parse(e.data);
                    if (data.type === 'self:debug') {
                        for (var entry of data.log) {
                            entry.time = new Date(entry.time);
                        }
                    }
                    if (selfDebug)
                        selfDebug.log('before', JSON.stringify(data));
                    handlerArguments.unshift(data);
                }
                for (var handler of handlersByKey) {
                    handler.apply(null, handlerArguments);
                }
                if (selfDebug && keyFixed === 'message')
                    selfDebug.log('after', JSON.stringify(handlerArguments[0]));
            });
        }
    }

    function tryToReopen() {
        if (mustBeClosed || reopening)
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

    function sendWhenOpen(command) {
        if (mustBeClosed)
            throw "Cannot send command '" + command + "' after the close() call.";
        return openPromise.then(function () {
            if (selfDebug)
                selfDebug.log('send', command);
            socket.send(command);
        });
    }

    this.on = function(key, handler) {
        handlers[key].push(handler);
    };
    this.off = function(key, handler) {
        const list = handlers[key];
        const index = list.indexOf(handler);
        if (index >= 0)
            list.splice(index, 1);
    };

    const removeEvents = addEvents(this, {
        error: tryToReopen,
        close: tryToReopen
    });
    this.sendReplaceText = function (start, length, newText, cursorIndexAfter, reason) {
        return sendWhenOpen('R' + start + ':' + length + ':' + cursorIndexAfter + ':' + (reason || '') + ':' + newText);
    };

    this.sendMoveCursor = function(cursorIndex) {
        return sendWhenOpen('M' + cursorIndex);
    };

    this.sendTypeChar = function(char) {
        return sendWhenOpen('C' + char);
    };

    const stateCommandMap = { cancel: 'X', force: 'F' };
    this.sendCompletionState = function(indexOrCommand) {
        const argument = stateCommandMap[indexOrCommand] || indexOrCommand;
        return sendWhenOpen('S' + argument);
    };

    this.sendSignatureHelpState = function(command) {
        return sendWhenOpen('P' + stateCommandMap[command]);
    };

    this.sendRequestInfoTip = function(active, cursorIndex) {
        return sendWhenOpen('I' + (active ? 'A' : 'N') + cursorIndex);
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
            optionPairs.push(key + '=' + options[key]);
        }
        return sendWhenOpen('O' + optionPairs.join(','));
    };

    this.sendRequestSelfDebugData = function() {
        return sendWhenOpen('Y');
    };

    this.close = function() {
        mustBeClosed = true;
        removeEvents();
        socket.close();
    };
}