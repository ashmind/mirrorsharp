import { ViewPlugin, showTooltip } from '@codemirror/view';
import type { Connection } from '../../connection';
import { addEvents } from '../../../helpers/add-events';
import type { SignatureData, SignatureInfoData, SignatureInfoParameterData, SignaturesEmptyMessage, SignaturesMessage } from '../../../interfaces/protocol';
import { renderPartsTo } from '../../../helpers/render-parts';
import { defineEffectField } from '../../../helpers/define-effect-field';

const [currentMessage, dispatchCurrentMessageChanged] = defineEffectField<SignaturesMessage | SignaturesEmptyMessage | undefined>();

const receiveSignatureHelpFromServer = (connection: Connection) => ViewPlugin.define(view => {
    const removeEvents = addEvents(connection, {
        message: message => {
            if (message.type !== 'signatures')
                return;

            dispatchCurrentMessageChanged(view, message);
        }
    });

    return {
        destroy: removeEvents
    };
});

const renderInfoTo = (parent: HTMLElement, { parts, parameter }: SignatureInfoData) => {
    if (parts.length > 0) {
        const element = document.createElement('div');
        element.className = 'mirrorsharp-signature-info';
        renderPartsTo(element, parts);
        parent.appendChild(element);
    }

    if (!parameter)
        return;

    renderInfoParameterTo(parent, parameter);
};

const renderInfoParameterTo = (parent: HTMLElement, parameter: SignatureInfoParameterData) => {
    if (parameter.parts.length === 0)
        return;

    const element = document.createElement('div');
    element.className = 'mirrorsharp-signature-info-parameter';

    const nameElement = document.createElement('span');
    nameElement.className = 'mirrorsharp-signature-info-parameter-name';
    nameElement.innerText = parameter.name + ': ';
    element.appendChild(nameElement);
    renderPartsTo(element, parameter.parts);

    parent.appendChild(element);
};

const renderSignatureList = (signatures: ReadonlyArray<SignatureData>) => {
    const list = document.createElement('ol');
    list.className = 'mirrorsharp-signature-list';

    for (const { parts, selected, info } of signatures) {
        const item = document.createElement('li');
        item.className = 'mirrorsharp-signature' + (selected ? ' mirrorsharp-signature-selected' : '');

        renderPartsTo(item, parts, {
            getExtraClassNames: part => ((part.selected) ? ['mirrorsharp-signature-part-selected'] : [])
        });
        if (info)
            renderInfoTo(item, info);
        list.appendChild(item);
    }

    return list;
};

const convertSignatureHelpToTooltip = showTooltip.from(currentMessage, message => {
    if (!message?.signatures)
        return null;

    const { span } = message;
    return {
        pos: span.start,
        end: span.start + span.length,
        create: () => ({ dom: renderSignatureList(message.signatures) })
    };
});

export const signatureHelpFromServer = (connection: Connection) => [
    currentMessage,
    receiveSignatureHelpFromServer(connection),
    convertSignatureHelpToTooltip
];