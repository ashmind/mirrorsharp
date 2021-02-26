import { TestDriver as IsomorphicTestDriver, TestDriverConstructorArguments, TestDriverOptions } from './test-driver-isomorphic';
import render from './helpers/render';

IsomorphicTestDriver.timers = jest;

const renderSize = { width: 320, height: 200 };

(() => {
    // clean JSDOM between tests
    const emptyHTML = document.body.innerHTML;
    afterEach(() => document.body.innerHTML = emptyHTML);
})();

Range.prototype.getBoundingClientRect = () => ({}) as unknown as DOMRect;
Range.prototype.getClientRects = () => [] as unknown as DOMRectList;

class TestDriver<TExtensionServerOptions = never> extends IsomorphicTestDriver<TExtensionServerOptions> {
    private constructor(...args: TestDriverConstructorArguments<TExtensionServerOptions>) {
        super(...args);
    }

    async render(options?: Parameters<typeof render>[2]): ReturnType<typeof render> {
        return await render(this, renderSize, options);
    }

    static async new<TExtensionServerOptions = never, TSlowUpdateExtensionData = never>(
        options: TestDriverOptions<TExtensionServerOptions, TSlowUpdateExtensionData>
    ) {
        return super.new(options) as unknown as TestDriver<TExtensionServerOptions>;
    }
}

let savedWebSocket: (typeof globalThis.WebSocket)|undefined;
beforeEach(() => {
    savedWebSocket = globalThis.WebSocket;
});

afterEach(() => {
    globalThis.WebSocket = savedWebSocket!;
});

export { TestDriver };