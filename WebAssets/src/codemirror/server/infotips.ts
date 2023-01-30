import { EditorView, ViewPlugin, hoverTooltip, Tooltip } from '@codemirror/view';
import { defineEffectField } from '../../helpers/define-effect-field';
import { renderPartsTo } from '../../helpers/render-parts';
import type { Connection } from '../../protocol/connection';
import type { InfotipMessage } from '../../protocol/messages';
import { convertFromServerPosition, convertToServerPosition, getEnd } from '../helpers/convert-position';

const [lastInfotipRequest, dispatchLastInfotipRequestChanged] = defineEffectField<{
    pos: number;
    promise: Promise<Tooltip>;
    resolve: ((tooltip: Tooltip) => void)
} | null>(null);

const kindsToClassNames = (kinds: ReadonlyArray<string>) => {
    return kinds.map(kind => 'mirrorsharp-infotip-icon-' + kind);
};

const renderInfotip = ({ sections, kinds }: InfotipMessage) => {
    const wrapper = document.createElement('div');
    wrapper.classList.add('mirrorsharp-infotip');
    sections.forEach((section, index) => {
        const element = document.createElement('div');
        element.className = 'mirrorsharp-parts-section';
        if (index === 0) {
            const icon = document.createElement('span');
            icon.classList.add('mirrorsharp-infotip-icon', ...kindsToClassNames(kinds));
            element.appendChild(icon);
        }

        renderPartsTo(element, section.parts);
        wrapper.appendChild(element);
    });
    return wrapper;
};

export const infotipsFromServer = <O, U>(connection: Connection<O, U>) => {
    const requestInfotip = (view: EditorView, pos: number) => {
        const lastRequest = view.state.field(lastInfotipRequest);
        if (lastRequest?.pos === pos)
            return lastRequest.promise;

        let resolve: (tooltip: Tooltip) => void;
        const promise = new Promise<Tooltip>(r => resolve = r);
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        dispatchLastInfotipRequestChanged(view, { pos, promise, resolve: resolve! });
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        connection.sendRequestInfoTip(convertToServerPosition(view.state.doc, pos));
        return promise;
    };

    const receiveInfotipFromServer = ViewPlugin.define(view => {
        const removeListeners = connection.addEventListeners({
            message: message => {
                if (message.type !== 'infotip')
                    return;

                const request = view.state.field(lastInfotipRequest);
                if (!request || !message.sections)
                    return;

                const { span } = message;
                const start = convertFromServerPosition(view.state.doc, span.start);
                const end = convertFromServerPosition(view.state.doc, getEnd(span.start, span.length));
                if (request.pos < start || request.pos > end)
                    return;

                request.resolve({
                    pos: request.pos,
                    end,
                    create: () => ({ dom: renderInfotip(message) })
                });
            }
        });

        return {
            destroy: removeListeners
        };
    });

    return [
        lastInfotipRequest,
        hoverTooltip(requestInfotip),
        receiveInfotipFromServer
    ];
};