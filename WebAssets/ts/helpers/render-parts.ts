import type { PartData } from '../interfaces/protocol';

/*
    const partKindClassMap = {
        text: 'mirrorsharp-tip-part-text',
        class: 'cm-type',
        struct: 'cm-type'
    };
*/

function createLine() {
    const line = document.createElement('div');
    line.className = 'mirrorsharp-parts-line';
    return line;
}

export function renderPartTo(parent: HTMLElement, part: PartData) {
    const span = document.createElement('span');
    span.className = 'cmt-' + part.kind;
    span.textContent = part.text;
    parent.appendChild(span);
}

export function renderPartsTo(parent: HTMLElement, parts: ReadonlyArray<PartData>) {
    let currentLine = createLine();
    for (const part of parts) {
        if (part.kind === 'linebreak') {
            parent.appendChild(currentLine);
            currentLine = createLine();
            continue;
        }
        renderPartTo(currentLine, part);
    }
    parent.appendChild(currentLine);
}

export function renderParts(parts: ReadonlyArray<PartData>): HTMLElement {
    const container = document.createElement('div');
    renderPartsTo(container, parts);
    return container;
}