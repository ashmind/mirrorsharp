import type { PartData } from '../protocol/messages';

/*
    const partKindClassMap = {
        text: 'mirrorsharp-tip-part-text',
        class: 'cm-type',
        struct: 'cm-type'
    };
*/

type Options<TPartData> = {
    splitLinesToSections?: boolean;
    getExtraClassNames?: (part: TPartData) => ReadonlyArray<string>;
};

const createSection = () => {
    const section = document.createElement('div');
    section.className = 'mirrorsharp-parts-section';
    return section;
};

const renderPartTo = <TPartData extends PartData>(
    parent: HTMLElement, part: TPartData,
    { getExtraClassNames }: Pick<Options<TPartData>, 'getExtraClassNames'>
) => {
    const span = document.createElement('span');
    const extraClassNames = getExtraClassNames?.(part);
    span.className = 'tok-' + part.kind + (extraClassNames ? ' ' + extraClassNames.join(' ') : '');
    span.textContent = part.text;
    parent.appendChild(span);
};

export const renderPartsTo = <TPartData extends PartData>(
    parent: HTMLElement,
    parts: ReadonlyArray<TPartData>,
    { splitLinesToSections, getExtraClassNames }: Options<TPartData> = {}
) => {
    let section = splitLinesToSections ? createSection() : parent;
    for (const part of parts) {
        if (part.kind === 'linebreak' && splitLinesToSections) {
            parent.appendChild(section);
            section = createSection();
            continue;
        }
        renderPartTo(section, part, { getExtraClassNames });
    }
    if (splitLinesToSections)
        parent.appendChild(section);
};

export const renderParts = <TPartData extends PartData>(
    parts: ReadonlyArray<TPartData>,
    options: Options<TPartData> = {}
): HTMLElement => {
    const container = document.createElement('div');
    renderPartsTo(container, parts, options);
    return container;
};