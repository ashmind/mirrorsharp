import { EditorView, ViewPlugin, hoverTooltip, Tooltip } from '@codemirror/view';
import { defineEffectField } from '../../helpers/define-effect-field';
import { renderPartsTo } from '../../helpers/render-parts';
import type { Connection } from '../../protocol/connection';
import type { InfotipMessage } from '../../protocol/messages';

const [lastInfotipRequest, dispatchLastInfotipRequestChanged] = defineEffectField<{ pos: number; resolve: ((tooltip: Tooltip) => void) }|null>(null);

const kindsToClassNames = (kinds: ReadonlyArray<string>) => {
    return kinds.map(kind => 'mirrorsharp-infotip-icon-' + kind);
};

const renderInfotip = ({ sections, kinds }: InfotipMessage) => {
    const wrapper = document.createElement('div');
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
        const infotip = new Promise<Tooltip>(resolve => {
            dispatchLastInfotipRequestChanged(view, { pos, resolve });
        });
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        connection.sendRequestInfoTip(pos);
        return infotip;
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
                if (request.pos < span.start || request.pos > span.start + span.length)
                    return;

                request.resolve({
                    pos: request.pos,
                    end: span.start + span.length,
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