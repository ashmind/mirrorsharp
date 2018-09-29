/* globals kindsToClassName:false */

function InfoTipRender() {
    const partKindClassMap = {
        text: 'mirrorsharp-tip-part-text',
        class: 'cm-type',
        struct: 'cm-type'
    };

    function renderSection(mainElement, section, index, info) {
        const element = document.createElement('div');
        element.className = 'mirrorsharp-tip-' + section.kind;
        if (index === 0)
            element.className += ' ' + kindsToClassName(info.kinds);
        section.parts.forEach(function(part) { renderPart(element, part); });
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
        data.sections.forEach(function (section, index) {
            renderSection(wrapper, section, index, data);
        });
        parent.appendChild(wrapper);
    };
}

/* exported InfoTipRender */