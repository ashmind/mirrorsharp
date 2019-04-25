/* globals CodeMirror:false, addEvents:false, kindsToClassName:false, renderParts:false */

/**
 * @this {internal.Hinter}
 * @param {CodeMirror.Editor} cm
 * @param {internal.Connection} connection
 * */
function Hinter(cm, connection) {
    const indexInListKey = '$mirrorsharp-indexInList';
    const priorityKey = '$mirrorsharp-priority';
    const cachedInfoKey = '$mirrorsharp-cached-info';

    /** @type {'starting'|'started'|'stopped'|'committed'} */
    var state = 'stopped';
    /** @type {boolean} */
    var hasSuggestion;
    /** @type {internal.CompletionOptionalData} */
    var currentOptions;
    /** @type {{ item: internal.HintEx, index: number, element: HTMLElement }} */
    var selected;

    /** @type {number} */
    var infoLoadTimer;

    /** @type {internal.HintEx['hint']} */
    const commit = function (_, data, item) {
        connection.sendCompletionState(item[indexInListKey]);
        state = 'committed';
    };

    const cancel = function() {
        if (cm.state.completionActive)
            cm.state.completionActive.close();
    };

    const removeCMEvents = addEvents(cm, {
        /**
         * @param {any} _
         * @param {KeyboardEvent} e
         * */
        keypress: function(_, e) {
            if (state === 'stopped')
                return;
            const key = e.key || String.fromCharCode(e.charCode || e.keyCode);
            if (currentOptions.commitChars.indexOf(key) > -1) {
                const widget = cm.state.completionActive.widget;
                if (!widget) {
                    cancel();
                    return;
                }
                widget.pick();
            }
        },

        endCompletion: function() {
            if (state === 'starting')
                return;
            if (state === 'started')
                connection.sendCompletionState('cancel');
            state = 'stopped';
            hideInfoTip();
            if (infoLoadTimer)
                clearTimeout(infoLoadTimer);
        }
    });

    /**
     * @param {ReadonlyArray<internal.CompletionItemData>} list
     * @param {internal.SpanData} span
     * @param {internal.CompletionOptionalData} options
     */
    this.start = function(list, span, options) {
        state = 'starting';
        currentOptions = options;
        const hintStart = cm.posFromIndex(span.start);
        const hintList = list.map(function (c, index) {
            /** @type {internal.HintEx} */
            const item = {
                text: c.filterText,
                displayText: c.displayText,
                // [TEMP] c.tags is for backward compatibility with 0.10
                className: 'mirrorsharp-hint ' + kindsToClassName(c.kinds || c.tags),
                hint: commit
            };
            item[indexInListKey] = index;
            item[priorityKey] = c.priority;
            if (c.span)
                item.from = cm.posFromIndex(c.span.start);
            return item;
        });
        const suggestion = options.suggestion;
        hasSuggestion = !!suggestion;
        if (hasSuggestion) {
            hintList.unshift({
                displayText: suggestion.displayText,
                className: 'mirrorsharp-hint mirrorsharp-hint-suggestion',
                hint: cancel
            });
        }
        cm.showHint({
            hint: function() { return getHints(hintList, hintStart); },
            completeSingle: false
        });
        state = 'started';
    };

    this.showTip = showInfoTip;

    this.destroy = function() {
        cancel();
        removeCMEvents();
    };

    /**
     * @param {ReadonlyArray<internal.HintEx>} list
     * @param {CodeMirror.Pos} start
     */
    function getHints(list, start) {
        const prefix = cm.getRange(start, cm.getCursor());
        if (prefix.length > 0) {
            var regexp = new RegExp('^' + prefix.replace(/[-/\\^$*+?.()|[\]{}]/g, '\\$&'), 'i');
            list = list.filter(function(item, index) {
                return (hasSuggestion && index === 0) || regexp.test(item.text);
            });
            if (hasSuggestion && list.length === 1)
                list = [];
        }
        if (!hasSuggestion) {
            // does not seem like I can use selectedHint here, as it does not force the scroll
            var selectedIndex = indexOfItemWithMaxPriority(list);
            if (selectedIndex > 0)
                setTimeout(function() { cm.state.completionActive.widget.changeActive(selectedIndex); }, 0);
        }

        /** @type {internal.HintsResultEx} */
        const result = { from: start, list: list };
        CodeMirror.on(result, 'select', loadInfo);

        return result;
    }

    /**
     * @param {internal.HintEx} item
     * @param {HTMLElement} element
     */
    function loadInfo(item, element) {
        selected = { item: item, index: item[indexInListKey], element: element };
        if (infoLoadTimer)
            clearTimeout(infoLoadTimer);

        if (item[cachedInfoKey])
            showInfoTip(selected.index, item[cachedInfoKey]);

        infoLoadTimer = setTimeout(function() {
            connection.sendCompletionState('info', selected.index);
            clearTimeout(infoLoadTimer);
        }, 300);
    }

    /** @type {HTMLDivElement} */
    var infoTipElement;
    /** @type {number} */
    var currentInfoTipIndex;

    /**
     * @param {number} index
     * @param {ReadonlyArray<internal.PartData>} parts
     */
    function showInfoTip(index, parts) {
        // autocompletion disappeared while we were loading
        if (state !== 'started')
            return;

        // selected index changed while we were loading
        if (index !== selected.index)
            return;

        // we are already showing tooltip for this index
        if (index === currentInfoTipIndex)
            return;

        selected.item[cachedInfoKey] = parts;

        var element = infoTipElement;
        if (!element) {
            element = document.createElement('div');
            element.className = 'mirrorsharp-hint-info-tooltip mirrorsharp-any-tooltip mirrorsharp-theme';
            element.style.display = 'none';
            document.body.appendChild(element);
            infoTipElement = element;
        }
        else {
            while (element.firstChild) {
                element.removeChild(element.firstChild);
            }
        }

        const top = selected.element.getBoundingClientRect().top;
        const left = selected.element.parentElement.getBoundingClientRect().right;
        const screenWidth = document.documentElement.getBoundingClientRect().width;

        const style = element.style;
        style.top = top + 'px';
        style.left = left + 'px';
        style.maxWidth = (screenWidth - left) + 'px';
        renderParts(infoTipElement, parts);
        style.display = 'block';

        currentInfoTipIndex = index;
    }

    function hideInfoTip() {
        currentInfoTipIndex = null;
        if (infoTipElement)
            infoTipElement.style.display = 'none';
    }

    /**
     * @param {ReadonlyArray<internal.HintEx>} list
     */
    function indexOfItemWithMaxPriority(list) {
        var maxPriority = 0;
        var result = 0;
        for (var i = 0; i < list.length; i++) {
            const priority = list[i][priorityKey];
            if (priority > maxPriority) {
                result = i;
                maxPriority = priority;
            }
        }
        return result;
    }
}

/* exported Hinter */