import type { PartData, CompletionItemData, SpanData } from '../interfaces/protocol';
import type { Hinter as HinterInterface, CompletionOptionalData, Hint } from '../interfaces/hinter';
import type { Connection } from '../interfaces/connection';
import CodeMirror from 'codemirror';
import 'codemirror/addon/hint/show-hint';
import { addEvents } from '../helpers/add-events';
import { renderParts } from '../helpers/render-parts';
import { kindsToClassName } from '../helpers/kinds-to-class-name';

type CompletionActiveState = {
    readonly widget?: CodeMirror.Handle & {
        changeActive(i: number, avoidWrap?: boolean): void;
    };
    close(): void;
}

function Hinter<TExtensionServerOptions, TSlowUpdateExtensionData>(
    this: HinterInterface,
    cm: CodeMirror.Editor,
    connection: Connection<TExtensionServerOptions, TSlowUpdateExtensionData>
) {
    const indexInListKey = '$mirrorsharp-indexInList';
    const priorityKey = '$mirrorsharp-priority';
    const cachedInfoKey = '$mirrorsharp-cached-info';

    let state: 'starting'|'started'|'stopped'|'committed' = 'stopped';
    let hasSuggestion: boolean;
    let currentOptions: CompletionOptionalData;
    let selected: { item: Hint; index: number; element: HTMLElement };

    let infoLoadTimer: ReturnType<typeof setTimeout>;

    // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
    const getActiveCompletion = () => cm.state.completionActive as CompletionActiveState|undefined;

    const commit = (_: CodeMirror.Editor, data: {}, item: Hint) => {
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion, @typescript-eslint/no-floating-promises
        connection.sendCompletionState(item[indexInListKey]!);
        state = 'committed';
    };

    const cancel = () => {
        const activeCompletion = getActiveCompletion();
        if (activeCompletion)
            activeCompletion.close();
    };

    const removeCMEvents = addEvents(cm, {
        keypress(_: unknown, e: KeyboardEvent) {
            if (state === 'stopped')
                return;
            const key = e.key || String.fromCharCode(e.charCode || e.keyCode);
            if (currentOptions.commitChars.includes(key)) {
                const widget = getActiveCompletion()?.widget;
                if (!widget) {
                    cancel();
                    return;
                }
                widget.pick();
            }
        },

        endCompletion() {
            if (state === 'starting')
                return;
            if (state === 'started') {
                // eslint-disable-next-line @typescript-eslint/no-floating-promises
                connection.sendCompletionState('cancel');
            }
            state = 'stopped';
            hideInfoTip();
            if (infoLoadTimer)
                clearTimeout(infoLoadTimer);
        }
    });

    this.start = function(list: ReadonlyArray<CompletionItemData>, span: SpanData, options: CompletionOptionalData) {
        state = 'starting';
        currentOptions = options;
        const hintStart = cm.posFromIndex(span.start);
        const hintList = list.map((c, index) => {
            const item = {
                text: c.filterText,
                displayText: c.displayText,
                className: 'mirrorsharp-hint ' + kindsToClassName(c.kinds),
                hint: commit
            } as Hint;
            item[indexInListKey] = index;
            item[priorityKey] = c.priority;
            if (c.span)
                item.from = cm.posFromIndex(c.span.start);
            return item;
        });
        const suggestion = options.suggestion;
        if (suggestion) {
            hintList.unshift({
                displayText: suggestion.displayText,
                className: 'mirrorsharp-hint mirrorsharp-hint-suggestion',
                hint: cancel
            });
        }
        cm.showHint({
            hint: () => getHints(hintList, hintStart),
            completeSingle: false
        });
        state = 'started';
    };

    this.showTip = showInfoTip;

    this.destroy = function() {
        cancel();
        removeCMEvents();
    };

    function getHints(list: ReadonlyArray<Hint>, start: CodeMirror.Position) {
        const prefix = cm.getRange(start, cm.getCursor());
        if (prefix.length > 0) {
            const regexp = new RegExp('^' + prefix.replace(/[-/\\^$*+?.()|[\]{}]/g, '\\$&'), 'i');
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            list = list.filter((item, index) => (hasSuggestion && index === 0) || regexp.test(item.text!));
            if (hasSuggestion && list.length === 1)
                list = [];
        }
        if (!hasSuggestion) {
            // does not seem like I can use selectedHint here, as it does not force the scroll
            const selectedIndex = indexOfItemWithMaxPriority(list);
            if (selectedIndex > 0)
                setTimeout(() => getActiveCompletion()?.widget?.changeActive(selectedIndex), 0);
        }

        const result = { from: start, list };
        CodeMirror.on(result, 'select', loadInfo);

        return result;
    }

    function loadInfo(item: Hint, element: HTMLElement) {
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        selected = { item, index: item[indexInListKey]!, element };
        if (infoLoadTimer)
            clearTimeout(infoLoadTimer);

        if (item[cachedInfoKey]) {
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            showInfoTip(selected.index, item[cachedInfoKey]!);
        }

        infoLoadTimer = setTimeout(() => {
            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            connection.sendCompletionState('info', selected.index);
            clearTimeout(infoLoadTimer);
        }, 300);
    }

    let infoTipElement: HTMLDivElement|undefined;
    let currentInfoTipIndex: number|undefined|null;

    function showInfoTip(index: number, parts: ReadonlyArray<PartData>) {
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

        let element = infoTipElement;
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
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        const left = selected.element.parentElement!.getBoundingClientRect().right;
        const screenWidth = document.documentElement.getBoundingClientRect().width;

        const style = element.style;
        style.top = top + 'px';
        style.left = left + 'px';
        style.maxWidth = (screenWidth - left) + 'px';
        renderParts(element, parts);
        style.display = 'block';

        currentInfoTipIndex = index;
    }

    function hideInfoTip() {
        currentInfoTipIndex = null;
        if (infoTipElement)
            infoTipElement.style.display = 'none';
    }

    function indexOfItemWithMaxPriority(list: ReadonlyArray<Hint>) {
        let maxPriority = 0;
        let result = 0;
        for (let i = 0; i < list.length; i++) {
            const priority = list[i][priorityKey];
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            if (priority! > maxPriority) {
                result = i;
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                maxPriority = priority!;
            }
        }
        return result;
    }
}

const HinterAsConstructor = Hinter as unknown as {
    new<TExtensionServerOptions, TSlowUpdateExtensionData>(cm: CodeMirror.Editor, connection: Connection<TExtensionServerOptions, TSlowUpdateExtensionData>): HinterInterface;
};

export { HinterAsConstructor as Hinter };