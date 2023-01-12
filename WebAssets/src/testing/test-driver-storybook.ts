import type { Theme } from '../interfaces/theme';
import { timers } from './storybook/browser-fake-timers';
import { MockSocketWithActionLog } from './storybook/mock-socket-with-action-log';
import { setTimers, TestDriverBase, TestDriverConstructorArguments, TestDriverOptions } from './test-driver-base';

setTimers(timers);
export class TestDriver<TExtensionServerOptions = never> extends TestDriverBase<TExtensionServerOptions> {
    static nextTheme?: Theme | null;
    #windowListenersToRemoveWhenDisablingEvents = [] as Array<{
        type: string;
        listener: EventListenerOrEventListenerObject;
    }>;

    private constructor(...args: TestDriverConstructorArguments<TExtensionServerOptions>) {
        super(...args);

        const savedAddEventListener = window.addEventListener;
        window.addEventListener = (...args: Parameters<typeof window.addEventListener>) => {
            const [type, listener] = args;
            savedAddEventListener(...args);
            this.#windowListenersToRemoveWhenDisablingEvents.push({ type, listener });
        };
    }

    protected static override newMockSocket() {
        return new MockSocketWithActionLog();
    }

    static override async new<TExtensionServerOptions = never, TSlowUpdateExtensionData = never>(
        options: TestDriverOptions<TExtensionServerOptions, TSlowUpdateExtensionData>
    ) {
        if (this.nextTheme) {
            if (options.theme)
                throw new Error('Cannot have both options.theme and nextTheme set');
            options = { ...options, theme: this.nextTheme };
        }

        return await super.new(options) as TestDriver;
    }

    disableAllFurtherInteractionEvents() {
        this.getCodeMirrorView().dom.style.pointerEvents = 'none';
        for (const { type, listener } of this.#windowListenersToRemoveWhenDisablingEvents) {
            window.removeEventListener(type, listener);
        }
    }
}