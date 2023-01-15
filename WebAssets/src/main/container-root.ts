import type { Theme } from './theme';

export class ContainerRoot {
    #element: HTMLDivElement;

    constructor(container: HTMLElement, codeMirrorDom: HTMLElement, theme: Theme) {
        this.#element = document.createElement('div');
        this.#element.classList.add('mirrorsharp');
        this.setThemeClass(theme);

        this.#element.appendChild(codeMirrorDom);

        container.appendChild(this.#element);
    }

    get element() {
        return this.#element;
    }

    setThemeClass(theme: Theme) {
        this.#element.classList.remove('mirrorsharp--theme-light', 'mirrorsharp--theme-dark');
        this.#element.classList.add(`mirrorsharp--theme-${theme}`);
    }

    destroy() {
        this.#element.remove();
    }
}