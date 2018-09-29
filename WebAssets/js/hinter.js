/* globals addEvents:false, kindsToClassName:false */

function Hinter(cm, connection) {
    const indexInListKey = '$mirrorsharp-indexInList';
    const priorityKey = '$mirrorsharp-priority';
    var state = 'stopped';
    var hasSuggestion;
    var currentOptions;

    const commit = function (_, data, item) {
        connection.sendCompletionState(item[indexInListKey]);
        state = 'committed';
    };

    const cancel = function() {
        if (cm.state.completionActive)
            cm.state.completionActive.close();
    };

    const removeCMEvents = addEvents(cm, {
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
        }
    });

    this.start = function(list, span, options) {
        state = 'starting';
        currentOptions = options;
        const hintStart = cm.posFromIndex(span.start);
        const hintList = list.map(function (c, index) {
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
            hint: function() {
                const prefix = cm.getRange(hintStart, cm.getCursor());
                // TODO: rename once this is covered by tests
                // eslint-disable-next-line no-shadow
                var list = hintList;
                if (prefix.length > 0) {
                    var regexp = new RegExp('^' + prefix.replace(/[-/\\^$*+?.()|[\]{}]/g, '\\$&'), 'i');
                    list = hintList.filter(function(item, index) {
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

                return { from: hintStart, list: list };
            },
            completeSingle: false
        });
        state = 'started';
    };

    this.destroy = function() {
        cancel();
        removeCMEvents();
    };

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