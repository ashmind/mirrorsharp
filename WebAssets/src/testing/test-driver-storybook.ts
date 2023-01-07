import { timers } from './storybook/browser-fake-timers';
import { setTimers, TestDriverBase, TestDriverOptions } from './test-driver-base';

setTimers(timers);
export class TestDriver extends TestDriverBase {
    static async new<TExtensionServerOptions = never, TSlowUpdateExtensionData = never>(
        options: TestDriverOptions<TExtensionServerOptions, TSlowUpdateExtensionData>
    ) {
        return await super.new(options) as TestDriver;
    }
}