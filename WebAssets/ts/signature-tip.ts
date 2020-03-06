import type { SpanData, SignatureData } from './interfaces/protocol';
import type { SignatureTip as SignatureTipInterface } from './interfaces/signature-tip';

function SignatureTip(this: SignatureTipInterface, cm: CodeMirror.Editor) {
    const displayKindToClassMap: {
        keyword: 'cm-keyword';
        [key: string]: string|undefined;
    } = {
        keyword: 'cm-keyword'
    };

    let active = false;
    let tooltip: HTMLDivElement;
    let ol: HTMLOListElement;

    const hide = () => {
        if (!active)
            return;

        document.body.removeChild(tooltip);
        active = false;
    };

    this.update = function(signatures: ReadonlyArray<SignatureData>, span: SpanData) {
        if (!tooltip) {
            tooltip = document.createElement('div');
            tooltip.className = 'mirrorsharp-theme mirrorsharp-any-tooltip mirrorsharp-signature-tooltip';
            ol = document.createElement('ol');
            tooltip.appendChild(ol);
        }

        if (!signatures || signatures.length === 0) {
            if (active)
                hide();
            return;
        }

        while (ol.firstChild) {
            ol.removeChild(ol.firstChild);
        }
        for (const signature of signatures) {
            const li = document.createElement('li');
            if (signature.selected)
                li.className = 'mirrorsharp-signature-selected';

            for (const part of signature.parts) {
                let className = displayKindToClassMap[part.kind] || '';
                if (part.selected)
                    className += ' mirrorsharp-signature-part-selected';

                let child;
                if (className) {
                    child = document.createElement('span');
                    child.className = className;
                    child.textContent = part.text;
                }
                else {
                    child = document.createTextNode(part.text);
                }
                li.appendChild(child);
            }
            ol.appendChild(li);
        }

        const startPos = cm.posFromIndex(span.start);

        active = true;

        const startCharCoords = cm.charCoords(startPos);
        tooltip.style.top = startCharCoords.bottom + 'px';
        tooltip.style.left = startCharCoords.left + 'px';
        document.body.appendChild(tooltip);
    };

    this.hide = hide;
}

const SignatureTipAsConstructor = SignatureTip as unknown as { new(cm: CodeMirror.Editor): SignatureTipInterface };

export { SignatureTipAsConstructor as SignatureTip };