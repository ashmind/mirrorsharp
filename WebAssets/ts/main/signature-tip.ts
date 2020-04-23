import type { SignatureData, SpanData } from '../interfaces/protocol';

// const displayKindToClassMap = {
//     keyword: 'cm-keyword'
// } as {
//     keyword: 'cm-keyword';
//     [key: string]: string|undefined;
// };

export class SignatureTip {
    // readonly #cm: CodeMirror.Editor;

    // #active = false;
    // #elements: { tooltip: HTMLDivElement; ol: HTMLOListElement }|undefined;

    constructor(/*cm: CodeMirror.Editor*/) {
        // this.#cm = cm;
    }

    update({ signatures, span }: { signatures: ReadonlyArray<SignatureData>; span: SpanData }|{ signatures?: undefined; span?: undefined }) {
        // let { tooltip, ol } = this.#elements ?? {};
        // if (!tooltip) {
        //     tooltip = document.createElement('div');
        //     tooltip.className = 'mirrorsharp-theme mirrorsharp-any-tooltip mirrorsharp-signature-tooltip';
        //     ol = document.createElement('ol');
        //     tooltip.appendChild(ol);

        //     this.#elements = { tooltip, ol };
        // }
        // else {
        //     // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        //     ol = ol!;
        // }

        // if (!signatures || signatures.length === 0) {
        //     if (this.#active)
        //         this.hide();
        //     return;
        // }

        // while (ol.firstChild) {
        //     ol.removeChild(ol.firstChild);
        // }
        // for (const signature of signatures) {
        //     const li = document.createElement('li');
        //     if (signature.selected)
        //         li.className = 'mirrorsharp-signature-selected';

        //     for (const part of signature.parts) {
        //         let className = displayKindToClassMap[part.kind] ?? '';
        //         if (part.selected)
        //             className += ' mirrorsharp-signature-part-selected';

        //         let child;
        //         if (className) {
        //             child = document.createElement('span');
        //             child.className = className;
        //             child.textContent = part.text;
        //         }
        //         else {
        //             child = document.createTextNode(part.text);
        //         }
        //         li.appendChild(child);
        //     }
        //     ol.appendChild(li);
        // }

        // // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        // const startPos = this.#cm.posFromIndex(span!.start);

        // this.#active = true;

        // const startCharCoords = this.#cm.charCoords(startPos);
        // tooltip.style.top = startCharCoords.bottom + 'px';
        // tooltip.style.left = startCharCoords.left + 'px';
        // document.body.appendChild(tooltip);
    }

    hide() {
        // if (!this.#active)
        //     return;

        // // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        // document.body.removeChild(this.#elements!.tooltip);
        // this.#active = false;
    }
}