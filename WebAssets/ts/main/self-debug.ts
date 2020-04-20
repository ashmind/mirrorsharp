import type { SelfDebugLogEntryData, SelfDebugMessage } from '../interfaces/protocol';

export interface SelfDebugConnection {
    sendRequestSelfDebugData(): Promise<void>;
}

export class SelfDebug {
    #getText: (() => string)|undefined;
    #getCursorIndex: (() => number)|undefined;
    #clientLog: Array<SelfDebugLogEntryData> = [];
    #clientLogSnapshot: Array<SelfDebugLogEntryData>|undefined;

    watchEditor(getText: () => string, getCursorIndex: () => number) {
        this.#getText = getText;
        this.#getCursorIndex = getCursorIndex;
    }

    log(event: string, message: string) {
        this.#clientLog.push({
            time: new Date(),
            event,
            message,
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            text: this.#getText!(),
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            cursor: this.#getCursorIndex!()
        });
        while (this.#clientLog.length > 100) {
            this.#clientLog.shift();
        }
    }

    requestData(connection: SelfDebugConnection) {
        this.#clientLogSnapshot = this.#clientLog.slice(0);
        return connection.sendRequestSelfDebugData();
    }

    displayData(serverData: SelfDebugMessage) {
        const log: Array<{ entry: SelfDebugLogEntryData; on: string; index: number }> = [];

        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        for (let i = 0; i < this.#clientLogSnapshot!.length; i++) {
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            log.push({ entry: this.#clientLogSnapshot![i], on: 'client', index: i });
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
    }
}