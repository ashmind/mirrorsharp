import { TestDriver as IsomorphicTestDriver, TestDriverConstructorArguments, TestDriverOptions, setTimers } from './test-driver-isomorphic';
import render, { shouldSkipRender } from './helpers/render';

const DEFAULT_SIZE = { width: 700, height: 300 };

(() => {
    // clean JSDOM between tests
    const emptyHTML = document.body.innerHTML;
    afterEach(() => document.body.innerHTML = emptyHTML);
})();

Range.prototype.getBoundingClientRect = () => ({}) as unknown as DOMRect;
Range.prototype.getClientRects = () => [{}] as unknown as DOMRectList;

export const timers = setTimers(jest);

export class TestDriver<TExtensionServerOptions = never> extends IsomorphicTestDriver<TExtensionServerOptions> {
    static readonly shouldSkipRender = shouldSkipRender;

    private constructor(...args: TestDriverConstructorArguments<TExtensionServerOptions>) {
        super(...args);
    }

    render({ size }: { size?: { width?: number; height?: number } } = {}): ReturnType<typeof render> {
        return render(this, {
            width: size?.width ?? DEFAULT_SIZE.width,
            height: size?.height ?? DEFAULT_SIZE.height
        });
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
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    globalThis.WebSocket = savedWebSocket!;
});