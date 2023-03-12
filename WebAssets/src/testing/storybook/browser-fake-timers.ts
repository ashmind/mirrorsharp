import { install } from '@sinonjs/fake-timers';
import type { TestDriverTimers } from '../test-driver-base';

const clock = install();

export const timers: TestDriverTimers = {
    setSystemTime(now?: number | Date): void {
        clock.setSystemTime(now);
    },

    runOnlyPendingTimers() {
        clock.runToLast();
    },

    advanceTimersByTime(msToRun: number) {
        clock.tick(msToRun);
    },

    advanceTimersToNextTimer() {
        throw new Error('Not implemented');
    }
};