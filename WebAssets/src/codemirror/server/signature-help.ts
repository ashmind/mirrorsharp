import { Prec } from '@codemirror/state';
import { ViewPlugin, showTooltip, keymap } from '@codemirror/view';
import { defineEffectField } from '../../helpers/define-effect-field';
import { renderPartsTo } from '../../helpers/render-parts';
import type { Connection } from '../../protocol/connection';
import type { SignatureData, SignatureInfoData, SignatureInfoParameterData, SignaturesEmptyMessage, SignaturesMessage } from '../../protocol/messages';
import { convertFromServerPosition, getEnd } from '../helpers/convert-position';

const [currentMessage, dispatchCurrentMessageChanged] = defineEffectField<SignaturesMessage | SignaturesEmptyMessage | undefined>();

const receiveSignatureHelpFromServer = (connection: Connection) => ViewPlugin.define(view => {
    const removeListeners = connection.addEventListeners({
        message: message => {
            if (message.type !== 'signatures')
                return;

            dispatchCurrentMessageChanged(view, message);
        }
    });

    return {
        destroy: removeListeners
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
        item.className = 'mirrorsharp-signature' + (selected ? ' mirrorsharp-signature--selected' : '');

        renderPartsTo(item, parts, {
            getExtraClassNames: part => ((part.selected) ? ['mirrorsharp-signature-part--selected'] : [])
        });
        if (info)
            renderInfoTo(item, info);
        list.appendChild(item);
    }

    return list;
};

const convertSignatureHelpToTooltip = showTooltip.compute([currentMessage, 'doc'], state => {
    const message = state.field(currentMessage);
    if (!message?.signatures)
        return null;

    const { span } = message;
    return {
        pos: convertFromServerPosition(state.doc, span.start),
        end: convertFromServerPosition(state.doc, getEnd(span.start, span.length)),
        create: () => ({ dom: renderSignatureList(message.signatures) })
    };
});

const forceSignatureHelpOnCtrlShiftSpace = (connection: Connection) => Prec.highest(keymap.of([{ key: 'Ctrl-Shift-Space', run: () => {
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    connection.sendSignatureHelpState('force');
    return true;
} }]));


export const signatureHelpFromServer = (connection: Connection) => [
    currentMessage,
    forceSignatureHelpOnCtrlShiftSpace(connection),
    receiveSignatureHelpFromServer(connection),
    convertSignatureHelpToTooltip
];