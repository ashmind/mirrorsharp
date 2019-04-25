/* globals addEvents:false */

/**
 * @this {internal.Connection}
 * @param {string} url
 * @param {internal.SelfDebug} selfDebug
 * */
function Connection(url, selfDebug) {
    /** @type {WebSocket} */
    var socket;
    /** @type {Promise} */
    var openPromise;
    const handlers = {
        /** @type {Array<Function>} */
        open:    [],
        /** @type {Array<Function>} */
        message: [],
        /** @type {Array<Function>} */
        error:   [],
        /** @type {Array<Function>} */
        close:   []
    };

    open();

    var mustBeClosed = false;
    var reopenPeriod = 0;
    /** @type {number} */
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
            // @ts-ignore
            const handlersByKey = handlers[key];
            socket.addEventListener(key, function (e) {
                const handlerArguments = [e];
                if (keyFixed === 'message') {
                    // @ts-ignore
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

    /**
     * @param {string} command
     */
    function sendWhenOpen(command) {
        if (mustBeClosed)
            throw "Cannot send command '" + command + "' after the close() call.";
        return openPromise.then(function () {
            if (selfDebug)
                selfDebug.log('send', command);
            socket.send(command);
        });
    }

    /**
     * @param {string} key
     * @param {Function} handler
     */
    this.on = function(key, handler) {
        // @ts-ignore
        handlers[key].push(handler);
    };
    /**
    * @param {string} key
    * @param {Function} handler
    */
    this.off = function(key, handler) {
        // @ts-ignore
        const list = handlers[key];
        const index = list.indexOf(handler);
        if (index >= 0)
            list.splice(index, 1);
    };

    const removeEvents = addEvents(this, {
        error: tryToReopen,
        close: tryToReopen
    });

    /**
    * @param {number} start
    * @param {number} length
    * @param {string} newText
    * @param {number} cursorIndexAfter
    * @param {string} reason
    */
    this.sendReplaceText = function (start, length, newText, cursorIndexAfter, reason) {
        return sendWhenOpen('R' + start + ':' + length + ':' + cursorIndexAfter + ':' + (reason || '') + ':' + newText);
    };

    /**
    * @param {number} cursorIndex
    */
    this.sendMoveCursor = function(cursorIndex) {
        return sendWhenOpen('M' + cursorIndex);
    };

    /**
    * @param {string} char
    */
    this.sendTypeChar = function(char) {
        return sendWhenOpen('C' + char);
    };

    /** @type {{ ['cancel']: 'X'; ['force']: 'F'; [index: number]: undefined }} */
    const stateCommandMap = { cancel: 'X', force: 'F' };

    /**
    * @param {internal.StateCommand|'info'|number} indexOrCommand
    * @param {number} [indexIfInfo]
    */
    this.sendCompletionState = function(indexOrCommand, indexIfInfo) {
        const argument = indexOrCommand !== 'info'
            ? (stateCommandMap[indexOrCommand] || indexOrCommand)
            : 'I' + indexIfInfo;
        return sendWhenOpen('S' + argument);
    };

    /**
    * @param {internal.StateCommand} command
    */
    this.sendSignatureHelpState = function(command) {
        return sendWhenOpen('P' + stateCommandMap[command]);
    };

    /**
    * @param {number} cursorIndex
    */
    this.sendRequestInfoTip = function(cursorIndex) {
        return sendWhenOpen('I' + cursorIndex);
    };

    this.sendSlowUpdate = function() {
        return sendWhenOpen('U');
    };

    /**
    * @param {number} actionId
    */
    this.sendApplyDiagnosticAction = function(actionId) {
        return sendWhenOpen('F' + actionId);
    };

    /**
    * @param {object} options
    */
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

/* exported Connection */