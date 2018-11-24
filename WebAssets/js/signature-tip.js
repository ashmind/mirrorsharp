/**
 * @this {internal.SignatureTip}
 * @param {CodeMirror.Editor} cm
 */
function SignatureTip(cm) {
    const displayKindToClassMap = {
        keyword: 'cm-keyword'
    };

    var active = false;
    /** @type {HTMLDivElement} */
    var tooltip;
    /** @type {HTMLOListElement} */
    var ol;

    const hide = function() {
        if (!active)
            return;

        document.body.removeChild(tooltip);
        active = false;
    };

    /**
     * @param {ReadonlyArray<internal.SignatureData>} signatures
     * @param {internal.SpanData} span
     */
    this.update = function(signatures, span) {
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
        for (var signature of signatures) {
            var li = document.createElement('li');
            if (signature.selected)
                li.className = 'mirrorsharp-signature-selected';

            for (var part of signature.parts) {
                var className = displayKindToClassMap[part.kind] || '';
                if (part.selected)
                    className += ' mirrorsharp-signature-part-selected';

                var child;
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

/* exported SignatureTip */