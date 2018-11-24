/*
    const partKindClassMap = {
        text: 'mirrorsharp-tip-part-text',
        class: 'cm-type',
        struct: 'cm-type'
    };
*/

/**
 * @param {HTMLElement} parent
 * @param {internal.PartData} part
 * @returns {void}
 * */
function renderPart(parent, part) {
    const span = document.createElement('span');
    span.className = 'cm-' + part.kind;
    span.textContent = part.text;
    parent.appendChild(span);
}

/**
 * @param {HTMLElement} parent
 * @param {ReadonlyArray<internal.PartData>} parts
 * @returns {void}
 * */
function renderParts(parent, parts) {
    parts.forEach(function(part) { renderPart(parent, part); });
}

/* exported renderParts, renderPart */