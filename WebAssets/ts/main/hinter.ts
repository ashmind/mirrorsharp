//import CodeMirror from 'codemirror';
//import 'codemirror/addon/hint/show-hint';
// import type { PartData, CompletionItemData, SpanData, CompletionSuggestionData } from '../interfaces/protocol';
// import type { Connection } from './connection';
// import { addEvents } from '../helpers/add-events';
// import { renderParts } from '../helpers/render-parts';
// import { kindsToClassName } from '../helpers/kinds-to-class-name';
/*
const indexInListKey = Symbol('mirrorsharp:indexInList');
const priorityKey = Symbol('mirrorsharp:priority');
const cachedInfoKey = Symbol('mirrorsharp:cachedInfo');

interface Hint {
    text?: string;
    displayText?: string;
    from?: CodeMirror.Position;
    className?: string;
    hint?: () => void;

    [indexInListKey]?: number;
    [priorityKey]?: number;
    [cachedInfoKey]?: ReadonlyArray<PartData>;
}

interface CompletionActiveState {
    readonly widget?: CodeMirror.Handle & {
        changeActive(i: number, avoidWrap?: boolean): void;
    };
    close(): void;
}

interface CompletionOptionalData {
    readonly suggestion?: CompletionSuggestionData;
    readonly commitChars: ReadonlyArray<string>;
}*/

export class Hinter<TExtensionServerOptions, TSlowUpdateExtensionData> {
    // readonly #cm: CodeMirror.Editor;
    // readonly #connection: Connection<TExtensionServerOptions, TSlowUpdateExtensionData>;

    // #state: 'starting'|'started'|'stopped'|'committed' = 'stopped';
    // #currentOptions: CompletionOptionalData|undefined;
    // #hasSuggestion: boolean|undefined;
    // #selected: { item: Hint; index: number; element: HTMLElement }|undefined;
    // #infoLoadTimer: ReturnType<typeof setTimeout>|undefined;

    // #removeCodeMirrorEvents: () => void;

    constructor(/*cm: CodeMirror.Editor, connection: Connection<TExtensionServerOptions, TSlowUpdateExtensionData>*/) {
        // this.#cm = cm;
        // this.#connection = connection;
        // this.#removeCodeMirrorEvents = addEvents(cm, {
        //     keypress: (_: unknown, e: KeyboardEvent) => {
        //         if (this.#state === 'stopped' || !this.#currentOptions)
        //             return;
        //         const key = e.key || String.fromCharCode(e.charCode || e.keyCode);
        //         if (this.#currentOptions.commitChars.includes(key)) {
        //             const widget = this.#getActiveCompletion()?.widget;
        //             if (!widget) {
        //                 this.#cancel();
        //                 return;
        //             }
        //             widget.pick();
        //         }
        //     },

        //     endCompletion: () => {
        //         if (this.#state === 'starting')
        //             return;
        //         if (this.#state === 'started') {
        //             // eslint-disable-next-line @typescript-eslint/no-floating-promises
        //             connection.sendCompletionState('cancel');
        //         }
        //         this.#state = 'stopped';
        //         this.#hideInfoTip();
        //         if (this.#infoLoadTimer)
        //             clearTimeout(this.#infoLoadTimer);
        //     }
        // });
    }

    // #getActiveCompletion = () => (this.#cm.state as { completionActive?: CompletionActiveState }).completionActive;

    // #commit = (_: CodeMirror.Editor, data: {}, item: Hint) => {
    //     // eslint-disable-next-line @typescript-eslint/no-non-null-assertion, @typescript-eslint/no-floating-promises
    //     this.#connection.sendCompletionState(item[indexInListKey]!);
    //     this.#state = 'committed';
    // };

    // #cancel = () => {
    //     const activeCompletion = this.#getActiveCompletion();
    //     if (activeCompletion)
    //         activeCompletion.close();
    // };

    // #getHints = (list: ReadonlyArray<Hint>, start: CodeMirror.Position) => {
    //     const prefix = this.#cm.getRange(start, this.#cm.getCursor());
    //     if (prefix.length > 0) {
    //         const regexp = new RegExp('^' + prefix.replace(/[-/\\^$*+?.()|[\]{}]/g, '\\$&'), 'i');
    //         // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    //         list = list.filter((item, index) => (this.#hasSuggestion && index === 0) || regexp.test(item.text!));
    //         if (this.#hasSuggestion && list.length === 1)
    //             list = [];
    //     }
    //     if (!this.#hasSuggestion) {
    //         // does not seem like I can use selectedHint here, as it does not force the scroll
    //         const selectedIndex = this.#indexOfItemWithMaxPriority(list);
    //         if (selectedIndex > 0)
    //             setTimeout(() => this.#getActiveCompletion()?.widget?.changeActive(selectedIndex), 0);
    //     }

    //     const result = { from: start, list };
    //     CodeMirror.on(result, 'select', this.#loadInfo);

    //     return result;
    // };

    // #loadInfo = (item: Hint, element: HTMLElement) => {
    //     // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    //     this.#selected = { item, index: item[indexInListKey]!, element };
    //     if (this.#infoLoadTimer)
    //         clearTimeout(this.#infoLoadTimer);

    //     if (item[cachedInfoKey]) {
    //         // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    //         this.#showInfoTip(this.#selected.index, item[cachedInfoKey]!);
    //     }

    //     this.#infoLoadTimer = setTimeout(() => {
    //         if (this.#selected) {
    //             // eslint-disable-next-line @typescript-eslint/no-floating-promises
    //             this.#connection.sendCompletionState('info', this.#selected.index);
    //         }
    //         if (this.#infoLoadTimer)
    //             clearTimeout(this.#infoLoadTimer);
    //     }, 300);
    // };

    // #infoTipElement: HTMLDivElement|undefined;
    // #currentInfoTipIndex: number|undefined|null;

    // #showInfoTip = (index: number, parts: ReadonlyArray<PartData>) => {
    //     // autocompletion disappeared while we were loading
    //     if (this.#state !== 'started' || !this.#selected)
    //         return;

    //     // selected index changed while we were loading
    //     if (index !== this.#selected.index)
    //         return;

    //     // we are already showing tooltip for this index
    //     if (index === this.#currentInfoTipIndex)
    //         return;

    //     this.#selected.item[cachedInfoKey] = parts;

    //     let element = this.#infoTipElement;
    //     if (!element) {
    //         element = document.createElement('div');
    //         element.className = 'mirrorsharp-hint-info-tooltip mirrorsharp-any-tooltip mirrorsharp-theme';
    //         element.style.display = 'none';
    //         document.body.appendChild(element);
    //         this.#infoTipElement = element;
    //     }
    //     else {
    //         while (element.firstChild) {
    //             element.removeChild(element.firstChild);
    //         }
    //     }

    //     const selectedElement = this.#selected.element;
    //     const selectedRect = selectedElement.getBoundingClientRect();
    //     // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    //     const parentElement = selectedElement.parentElement!;
    //     const parentRect = parentElement.getBoundingClientRect();

    //     const top = parentElement.offsetTop + (selectedRect.top - parentRect.top);
    //     const left = parentRect.right;
    //     const screenWidth = document.documentElement.getBoundingClientRect().width;

    //     const style = element.style;
    //     style.top = top + 'px';
    //     style.left = left + 'px';
    //     style.maxWidth = (screenWidth - left) + 'px';
    //     renderParts(element, parts);
    //     style.display = 'block';

    //     this.#currentInfoTipIndex = index;
    // };

    // #hideInfoTip = () => {
    //     this.#currentInfoTipIndex = null;
    //     if (this.#infoTipElement)
    //         this.#infoTipElement.style.display = 'none';
    // };

    // #indexOfItemWithMaxPriority = (list: ReadonlyArray<Hint>) => {
    //     let maxPriority = 0;
    //     let result = 0;
    //     for (let i = 0; i < list.length; i++) {
    //         // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    //         const priority = list[i][priorityKey]!;
    //         if (priority > maxPriority) {
    //             result = i;
    //             maxPriority = priority;
    //         }
    //     }
    //     return result;
    // };

    // start(list: ReadonlyArray<CompletionItemData>, span: SpanData, options: CompletionOptionalData) {
    //     this.#state = 'starting';
    //     this.#currentOptions = options;
    //     const hintStart = this.#cm.posFromIndex(span.start);
    //     const hintList = list.map((c, index) => {
    //         const item = {
    //             text: c.filterText,
    //             displayText: c.displayText,
    //             className: 'mirrorsharp-hint ' + kindsToClassName(c.kinds),
    //             hint: this.#commit
    //         } as Hint;
    //         item[indexInListKey] = index;
    //         item[priorityKey] = c.priority;
    //         if (c.span)
    //             item.from = this.#cm.posFromIndex(c.span.start);
    //         return item;
    //     });
    //     const suggestion = options.suggestion;
    //     if (suggestion) {
    //         hintList.unshift({
    //             displayText: suggestion.displayText,
    //             className: 'mirrorsharp-hint mirrorsharp-hint-suggestion',
    //             hint: this.#cancel
    //         });
    //     }
    //     this.#cm.showHint({
    //         hint: () => this.#getHints(hintList, hintStart),
    //         completeSingle: false
    //     });
    //     this.#state = 'started';
    // }

    // readonly showTip = this.#showInfoTip;

    // destroy() {
    //     this.#cancel();
    //     this.#removeCodeMirrorEvents();
    // }
}