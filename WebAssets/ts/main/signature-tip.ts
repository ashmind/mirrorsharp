// import type { SignatureData, SpanData, SignatureInfoParameterData } from '../interfaces/protocol';
// import { renderParts } from '../helpers/render-parts';

// const displayKindToClassMap = {
//     keyword: 'cm-keyword'
// } as {
//     keyword: 'cm-keyword';
//     [key: string]: string|undefined;
// };

// ts-unused-exports:disable-next-line
export class SignatureTip {
    // readonly #cm: CodeMirror.Editor;

    // #active = false;
    // #elements: { tooltip: HTMLDivElement; ol: HTMLOListElement }|undefined;

    constructor(/*cm: CodeMirror.Editor*/) {
        // this.#cm = cm;
    }

    // update({ signatures, span }: { signatures: ReadonlyArray<SignatureData>; span: SpanData }|{ signatures?: undefined; span?: undefined }) {
    //     let { tooltip, ol } = this.#elements ?? {};
    //     if (!tooltip) {
    //         tooltip = document.createElement('div');
    //         tooltip.className = 'mirrorsharp-theme mirrorsharp-any-tooltip mirrorsharp-signature-tooltip';
    //         ol = document.createElement('ol');
    //         tooltip.appendChild(ol);

    //         this.#elements = { tooltip, ol };
    //     }
    //     else {
    //         // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    //         ol = ol!;
    //     }

    //     if (!signatures || signatures.length === 0) {
    //         if (this.#active)
    //             this.hide();
    //         return;
    //     }

    //     while (ol.firstChild) {
    //         ol.removeChild(ol.firstChild);
    //     }
    //     for (const signature of signatures) {
    //         const li = document.createElement('li');
    //         if (signature.selected)
    //             li.className = 'mirrorsharp-signature-selected';

    //         for (const part of signature.parts) {
    //             let className = displayKindToClassMap[part.kind] ?? '';
    //             if (part.selected)
    //                 className += ' mirrorsharp-signature-part-selected';
    //             let child;
    //             if (className) {
    //                 child = document.createElement('span');
    //                 child.className = className;
    //                 child.textContent = part.text;
    //             }
    //             else {
    //                 child = document.createTextNode(part.text);
    //             }
    //             li.appendChild(child);
    //         }
    //         this.#renderInfo(li, signature);

    //         ol.appendChild(li);
    //     }

    //     // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    //     const startPos = this.#cm.posFromIndex(span!.start);

    //     this.#active = true;

    //     const startCharCoords = this.#cm.charCoords(startPos);
    //     tooltip.style.top = startCharCoords.bottom + 'px';
    //     tooltip.style.left = startCharCoords.left + 'px';
    //     document.body.appendChild(tooltip);
    // }

    // #renderInfo = (parent: HTMLElement, signature: SignatureData) => {
    //     const { info } = signature;
    //     if (!info)
    //         return;

    //     if (info.parts.length > 0) {
    //         const element = document.createElement('div');
    //         renderParts(element, info.parts);
    //         parent.appendChild(element);
    //     }

    //     const { parameter } = info;
    //     if (!parameter)
    //         return;

    //     this.#renderInfoParameter(parent, parameter);
    // };

    // #renderInfoParameter = (parent: HTMLElement, parameter: SignatureInfoParameterData) => {
    //     if (parameter.parts.length === 0)
    //         return;

    //     const element = document.createElement('div');
    //     element.className = 'mirrorsharp-signature-info-parameter';

    //     const nameElement = document.createElement('span');
    //     nameElement.className = 'mirrorsharp-signature-info-parameter-name';
    //     nameElement.innerText = parameter.name + ': ';
    //     element.appendChild(nameElement);
    //     renderParts(element, parameter.parts);

    //     parent.appendChild(element);
    // };

    hide() {
        // if (!this.#active)
        //     return;

        // // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        // document.body.removeChild(this.#elements!.tooltip);
        // this.#active = false;
    }
}