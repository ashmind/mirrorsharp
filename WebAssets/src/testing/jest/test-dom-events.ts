import type { EditorView } from '@codemirror/view';

export class TestDomEvents {
    readonly #cmView: EditorView;

    constructor(cmView: EditorView) {
        this.#cmView = cmView;
    }

    keydown(key: string, other: Omit<KeyboardEventInit, 'key'> = {}) {
        this.#cmView
            .contentDOM
            .dispatchEvent(new KeyboardEvent('keydown', { key, ...other }));
    }

    mousemove(target: Node) {
        const event = new MouseEvent('mousemove', { bubbles: true });
        // default does not apply fake timers due to global object differences
        Object.defineProperty(event, 'timeStamp', { value: Date.now() });
        target.dispatchEvent(event);
    }

    mouseover(selector: string) {
        const target = this.#cmView.dom.querySelector(selector);
        if (!target)
            throw new Error(`Could not find element '${selector}'.`);
        target.dispatchEvent(new MouseEvent('mouseover', { bubbles: true }));
    }
}