//import { Keyboard } from 'keysim';
import { TestDriver as IsomorphicTestDriver, TestDriverConstructorArguments, setTimers, TestDriverOptions } from './test-driver-isomorphic';
import render from './helpers/render';

setTimers(jest);

const renderSize = { width: 320, height: 200 };

(() => {
    // clean JSDOM between tests
    const emptyHTML = document.body.innerHTML;
    afterEach(() => document.body.innerHTML = emptyHTML);
})();

//const keyboard = Keyboard.US_ENGLISH;

//const spliceString = (string: string, start: number, length: number, newString = '') =>
//    string.substring(0, start) + newString + string.substring(start + length);

Range.prototype.getClientRects = () => { return [] as unknown as DOMRectList; };

/*
class TestKeys {
    readonly #cmView: EditorView;

    constructor(cmView: EditorView) {
        this.#cmView = cmView;
    }

    backspace(count: number) {
        const { node, offset } = this.getCursorInfo();
        for (let i = 0; i < count; i++) {
            node.textContent = spliceString(node.textContent!, offset, 1);
            keyboard.dispatchEventsForAction('backspace', this.#cmView.contentDOM);
        }
    }

    press(keys: string) {
        keyboard.dispatchEventsForAction(keys, this.#cmView.contentDOM);
    }

    private getCursorInfo() {
        const index = this.#cmView.state.selection.primary.from;
        return this.#cmView.domAtPos(index);
    }
}
*/

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