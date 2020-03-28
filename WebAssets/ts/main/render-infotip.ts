import type { InfotipMessage, InfotipSectionData } from '../interfaces/protocol';
import { kindsToClassName } from '../helpers/kinds-to-class-name';
import { renderParts } from '../helpers/render-parts';

export const renderInfotip = (() => {
    function renderSection(mainElement: HTMLElement, section: InfotipSectionData, index: number, info: InfotipMessage) {
        const element = document.createElement('div');
        element.className = 'mirrorsharp-tip-' + section.kind;
        if (index === 0)
            element.className += ' ' + kindsToClassName(info.kinds);
        renderParts(element, section.parts);
        mainElement.appendChild(element);
    }

    return function(parent: HTMLElement, data: InfotipMessage) {
        const wrapper = document.createElement('div');
        wrapper.className = 'mirrorsharp-theme mirrorsharp-tip-content';
        data.sections.forEach(
            (section, index) => renderSection(wrapper, section, index, data)
        );
        parent.appendChild(wrapper);
    };
})();