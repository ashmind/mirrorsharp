/* globals kindsToClassName:false */

function InfoTipRender() {
    const partKindClassMap = {
        text: 'mirrorsharp-tip-part-text',
        class: 'cm-type',
        struct: 'cm-type'
    };

    function renderEntry(mainElement, entry, index, info) {
        const element = document.createElement('div');
        element.className = 'mirrorsharp-tip-' + entry.kind;
        if (index === 0)
            element.className += ' ' + kindsToClassName(info.kinds);
        entry.parts.forEach(function(part) { renderPart(element, part); });
        mainElement.appendChild(element);
    }

    function renderPart(entryElement, part) {
        const span = document.createElement('span');
        span.className = partKindClassMap[part.kind] || 'cm-' + part.kind;
        span.textContent = part.text;
        entryElement.appendChild(span);
    }

    return function render(parent, data) {
        const wrapper = document.createElement('div');
        wrapper.className = 'mirrorsharp-theme mirrorsharp-tip';
        data.entries.forEach(function(entry, index) {
            renderEntry(wrapper, entry, index, data);
        });
        parent.appendChild(wrapper);
    };
}