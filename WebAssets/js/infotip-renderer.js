function InfoTipRenderer() {
    const skipFromSectionClass = /^(space|punctuation|text)$/;
    const partKindClassMap = {
        text: 'mirrorsharp-tip-part-text',
        class: 'cm-type',
        struct: 'cm-type'
    };

    function renderPart(section, part) {
        const span = document.createElement('span');
        span.className = partKindClassMap[part.kind] || 'cm-' + part.kind;
        if (!skipFromSectionClass.test(part.kind))
            section.classList.add('mirrorsharp-kind-' + part.kind);
        span.textContent = part.text;
        section.appendChild(span);
    }

    this.render = function(info) {
        return info.map(function(item) {
            const section = document.createElement('div');
            section.className = 'mirrorsharp-theme mirrorsharp-tip mirrorsharp-tip-' + item.kind;
            item.parts.forEach(function(part) { renderPart(section, part); });
            return section;
        });
    };
}