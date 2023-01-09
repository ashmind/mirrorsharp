import type { MirrorSharpInstance } from '../mirrorsharp';
import type { MockSocketController } from './shared/mock-socket';
import { timers } from './storybook/browser-fake-timers';
import { setTimers, TestDriverBase, TestDriverOptions } from './test-driver-base';

setTimers(timers);
export class TestDriver<TExtensionServerOptions = never> extends TestDriverBase<TExtensionServerOptions> {
    #windowListenersToRemoveWhenDisablingEvents = [] as Array<{
        type: string;
        listener: EventListenerOrEventListenerObject;
    }>;

    constructor(
        // https://github.com/microsoft/TypeScript/issues/30991
        socket: MockSocketController,
        mirrorsharp: MirrorSharpInstance<TExtensionServerOptions>,
        optionsForJSONOnly: TestDriverOptions<TExtensionServerOptions, unknown>
    ) {
        super(socket, mirrorsharp, optionsForJSONOnly);

        const savedAddEventListener = window.addEventListener;
        window.addEventListener = (...args: Parameters<typeof window.addEventListener>) => {
            const [type, listener] = args;
            savedAddEventListener(...args);
            this.#windowListenersToRemoveWhenDisablingEvents.push({ type, listener });
        };
    }

    static async new<TExtensionServerOptions = never, TSlowUpdateExtensionData = never>(
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