import { TestDomEvents } from './jest/test-dom-events';
import { TestText } from './jest/test-text';
import { TestDriverBase, TestDriverConstructorArguments, TestDriverOptions, setTimers } from './test-driver-base';

(() => {
    // clean JSDOM between tests
    const emptyHTML = document.body.innerHTML;
    afterEach(() => document.body.innerHTML = emptyHTML);
})();

Range.prototype.getBoundingClientRect = () => ({}) as unknown as DOMRect;
Range.prototype.getClientRects = () => [{}] as unknown as DOMRectList;

export const timers = setTimers(jest);

export class TestDriver<TExtensionServerOptions = never> extends TestDriverBase {
    public readonly text: TestText;
    public readonly domEvents: TestDomEvents;

    private constructor(...args: TestDriverConstructorArguments<TExtensionServerOptions>) {
        super(...args);

        const cmView = this.getCodeMirrorView();
        this.text = new TestText(cmView);
        this.domEvents = new TestDomEvents(cmView);
    }

    static async new<TExtensionServerOptions = never, TSlowUpdateExtensionData = never>(
        options: TestDriverOptions<TExtensionServerOptions, TSlowUpdateExtensionData>
    ) {
        return await super.new(options) as TestDriver<TExtensionServerOptions>;
    }
}

let savedWebSocket: (typeof globalThis.WebSocket)|undefined;
beforeEach(() => {
    savedWebSocket = globalThis.WebSocket;
});

afterEach(() => {
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    globalThis.WebSocket = savedWebSocket!;
});