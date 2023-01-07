export const dispatchMutation = (() => {
    if (!navigator.userAgent.includes('jsdom')) {
        // Should not be required when running in browser
        // eslint-disable-next-line @typescript-eslint/no-empty-function
        return () => {};
    }

    // workaround for https://github.com/jsdom/jsdom/issues/3096
    const mutationObserverCallbacks = new WeakMap<HTMLElement, Array<{ callback: MutationCallback; observer: MutationObserver }>>();
    window.MutationObserver = class {
        #callback: MutationCallback;
        #elements = [] as Array<HTMLElement>;

        constructor(callback: MutationCallback) {
            this.#callback = callback;
        }

        observe(element: HTMLElement) {
            let callbacks = mutationObserverCallbacks.get(element);
            if (!callbacks) {
                callbacks = [];
                mutationObserverCallbacks.set(element, callbacks);
            }
            callbacks.push({ callback: this.#callback, observer: this });
            this.#elements.push(element);
        }

        takeRecords(): Array<MutationRecord> {
            return [];
        }

        disconnect() {
            for (const element of this.#elements) {
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                const callbacks = mutationObserverCallbacks.get(element)!;
                callbacks.splice(callbacks.findIndex(({ callback }) => callback === this.#callback), 1);
            }
        }
    };

    return (element: HTMLElement, record: Partial<MutationRecord>) => {
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        for (const { callback, observer } of mutationObserverCallbacks.get(element)!) {
            callback([record as MutationRecord], observer);
        }
    };
})();