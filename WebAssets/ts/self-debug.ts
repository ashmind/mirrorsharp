import type { SelfDebug as SelfDebugInterface } from './interfaces/self-debug';
import type { Connection } from './interfaces/connection';
import type { SelfDebugLogEntryData, SelfDebugMessage } from './interfaces/protocol';

function SelfDebug<TExtensionData>(this: SelfDebugInterface<TExtensionData>) {
    let getText: () => string;
    let getCursorIndex: () => number;
    const clientLog: Array<SelfDebugLogEntryData> = [];
    let clientLogSnapshot: Array<SelfDebugLogEntryData>;

    this.watchEditor = function(getTextValue: () => string, getCursorIndexValue: () => number) {
        getText = getTextValue;
        getCursorIndex = getCursorIndexValue;
    };

    this.log = function(event: string, message: string) {
        clientLog.push({
            time: new Date(),
            event,
            message,
            text: getText(),
            cursor: getCursorIndex()
        });
        while (clientLog.length > 100) {
            clientLog.shift();
        }
    };

    this.requestData = function(connection: Connection<TExtensionData>) {
        clientLogSnapshot = clientLog.slice(0);
        connection.sendRequestSelfDebugData();
    };

    this.displayData = function(serverData: SelfDebugMessage) {
        const log: Array<{ entry: SelfDebugLogEntryData; on: string; index: number }> = [];

        for (let i = 0; i < clientLogSnapshot.length; i++) {
            log.push({ entry: clientLogSnapshot[i], on: 'client', index: i });
        }

        for (let i = 0; i < serverData.log.length; i++) {
            log.push({ entry: serverData.log[i], on: 'server', index: i });
        }

        log.sort((a, b) => {
            if (a.on !== b.on) {
                if (a.entry.time > b.entry.time) return +1;
                if (a.entry.time < b.entry.time) return -1;
                return 0;
            }
            if (a.index > b.index) return +1;
            if (a.index < b.index) return -1;
            return 0;
        });

        console.table(log.map(l => {
            const time = l.entry.time;
            const displayTime = ('0' + time.getHours()).slice(-2) + ':' + ('0' + time.getMinutes()).slice(-2) + ':' + ('0' + time.getSeconds()).slice(-2) + '.' + time.getMilliseconds();
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

const SelfDebugAsConstructor = SelfDebug as unknown as { new<TExtensionData>(): SelfDebugInterface<TExtensionData> };

export { SelfDebugAsConstructor as SelfDebug };