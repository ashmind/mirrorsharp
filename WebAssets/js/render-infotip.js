/* globals renderParts:false, kindsToClassName:false */

const renderInfotip = (function() {
    /**
     * @param {HTMLElement} mainElement
     * @param {internal.InfotipSectionData} section
     * @param {number} index
     * @param {internal.InfotipMessage} info
     */
    function renderSection(mainElement, section, index, info) {
        const element = document.createElement('div');
        element.className = 'mirrorsharp-tip-' + section.kind;
        if (index === 0)
            element.className += ' ' + kindsToClassName(info.kinds);
        renderParts(element, section.parts);
        mainElement.appendChild(element);
    }

    return function(/** @type {HTMLElement} */ parent, /** @type {internal.InfotipMessage} */ data) {
        const wrapper = document.createElement('div');
        wrapper.className = 'mirrorsharp-theme mirrorsharp-tip-content';
        data.sections.forEach(function (section, index) {
            renderSection(wrapper, section, index, data);
        });
        parent.appendChild(wrapper);
    };
})();

/* exported renderInfotip */