/**
 * @this {internal.SelfDebug}
 */
function SelfDebug() {
    /** @type {() => string} */
    var getText;
    /** @type {() => number} */
    var getCursorIndex;
    /** @type {Array<internal.SelfDebugLogEntryData>} */
    const clientLog = [];
    /** @type {Array<internal.SelfDebugLogEntryData>} */
    var clientLogSnapshot;

    /**
     * @param {() => string} getTextValue
     * @param {() => number} getCursorIndexValue
     */
    this.watchEditor = function(getTextValue, getCursorIndexValue) {
        getText = getTextValue;
        getCursorIndex = getCursorIndexValue;
    };

    /**
     * @param {string} event
     * @param {string} message
     */
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

    /**
     * @param {internal.Connection} connection
     */
    this.requestData = function(connection) {
        clientLogSnapshot = clientLog.slice(0);
        connection.sendRequestSelfDebugData();
    };

    /**
     * @param {internal.SelfDebugMessage} serverData
     */
    this.displayData = function(serverData) {
        /** @type {Array<{ entry: internal.SelfDebugLogEntryData, on: string, index: number }>} */
        const log = [];
        // ReSharper disable once DuplicatingLocalDeclaration
        /* eslint-disable block-scoped-var */
        for (var i = 0; i < clientLogSnapshot.length; i++) {
            log.push({ entry: clientLogSnapshot[i], on: 'client', index: i });
        }
        // ReSharper disable once DuplicatingLocalDeclaration
        // eslint-disable-next-line no-redeclare
        for (var i = 0; i < serverData.log.length; i++) {
            log.push({ entry: serverData.log[i], on: 'server', index: i });
        }
        /* eslint-enable block-scoped-var */
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

        console.table(log.map(function(l) { // eslint-disable-line no-console
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
    };
}

/* exported SelfDebug */