import { timers } from './storybook/browser-fake-timers';
import { setTimers, TestDriverBase, TestDriverConstructorArguments, TestDriverOptions } from './test-driver-base';

setTimers(timers);
export class TestDriver<TExtensionServerOptions = never> extends TestDriverBase<TExtensionServerOptions> {
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

    static override async new<TExtensionServerOptions = never, TSlowUpdateExtensionData = never>(
        options: TestDriverOptions<TExtensionServerOptions, TSlowUpdateExtensionData>
    ) {
        return await super.new(options) as TestDriver;
    }

    disableAllFurtherInteractionEvents() {
        this.getCodeMirrorView().dom.style.pointerEvents = 'none';
        for (const { type, listener } of this.#windowListenersToRemoveWhenDisablingEvents) {
            window.removeEventListener(type, listener);
        }
    }
}