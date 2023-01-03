import type { PartData } from '../interfaces/protocol';

/*
    const partKindClassMap = {
        text: 'mirrorsharp-tip-part-text',
        class: 'cm-type',
        struct: 'cm-type'
    };
*/

type Options = {
    splitLinesToSections?: boolean;
};

function createSection() {
    const section = document.createElement('div');
    section.className = 'mirrorsharp-parts-section';
    return section;
}

export function renderPartTo(parent: HTMLElement, part: PartData) {
    const span = document.createElement('span');
    span.className = 'tok-' + part.kind;
    span.textContent = part.text;
    parent.appendChild(span);
}

export function renderPartsTo(parent: HTMLElement, parts: ReadonlyArray<PartData>, { splitLinesToSections }: Options = {}) {
    let section = splitLinesToSections ? createSection() : parent;
    for (const part of parts) {
        if (part.kind === 'linebreak' && splitLinesToSections) {
            parent.appendChild(section);
            section = createSection();
            continue;
        }
        renderPartTo(section, part);
    }
    if (splitLinesToSections)
        parent.appendChild(section);
}

export function renderParts(parts: ReadonlyArray<PartData>, options: Options = {}): HTMLElement {
    const container = document.createElement('div');
    renderPartsTo(container, parts, options);
    return container;
}