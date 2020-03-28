import type { PartData } from '../interfaces/protocol';

/*
    const partKindClassMap = {
        text: 'mirrorsharp-tip-part-text',
        class: 'cm-type',
        struct: 'cm-type'
    };
*/

export function renderPart(parent: HTMLElement, part: PartData) {
    const span = document.createElement('span');
    span.className = 'cm-' + part.kind;
    span.textContent = part.text;
    parent.appendChild(span);
}

export function renderParts(parent: HTMLElement, parts: ReadonlyArray<PartData>) {
    for (const part of parts) {
        renderPart(parent, part);
    }
}