declare module CodeMirror {
    export interface EditorConfiguration {
        readonly lineSeparator?: string;
    }

    export interface Document {
        getValue(): string;
        setValue(value: string): void;
        getCursor(): Pos;
        getRange(from: Pos, to: Pos): string;
        replaceRange(replacement: string, from: Pos, to: Pos, origin: string): void;
        somethingSelected(): boolean;
        replaceSelection(replacement: string, select?: 'start'|'around'|'end'): void;
        posFromIndex(index: number): Pos;
        indexFromPos(pos: Pos): number;
        charCoords(pos: Pos): Coords;
    }

    export interface Editor extends Document {
        setOption(name: 'mode', value: string): void;
        setOption(name: 'lint', value: LintOptions): void;
        addKeyMap(keyMap: KeyMap): void;
        removeKeyMap(keyMap: KeyMap): void;
        on(type: 'beforeChange', func: (cm: Editor, change: Change) => void): void;
        off(type: 'beforeChange', func: (cm: Editor, change: Change) => void): void;

        getWrapperElement(): Element;
        save(): void;
        toTextArea(): void;
        readonly state: State;

        showHint(options: HintOptions): void;
    }

    export interface Element extends HTMLElement {
        readonly CodeMirror: Editor;
    }

    export interface Pos {
        readonly line: number;
        readonly ch: number;
    }

    export interface Change {
        readonly from: Pos;
        readonly to: Pos;
        readonly text: ReadonlyArray<string>;
        readonly origin: string;
    }

    export interface Coords {
        readonly bottom: number;
        readonly left: number;
    }

    interface KeyMap {
        [key: string]: string|(() => void)
    }

    interface State {
        readonly lint: LintState;
        readonly completionActive: CompletionActiveState;
    }

    interface CompletionActiveState {
        readonly widget: CompletionActiveWidget;
        close(): void;
    }

    interface CompletionActiveWidget {
        changeActive(index: number): void;
        pick(): void;
    }

    interface LintState {

    }

    interface LintOptions {
    }

    export interface LintAnnotation {
        readonly severity: string;
        readonly message: string;
        readonly from: Pos;
        readonly to: Pos;
    }

    interface HintOptions {
        hint: () => void;
        completeSingle: boolean;
    }

    interface Hint {
        text?: string;
        displayText: string;
        className: string;
        hint?: (cm: CodeMirror.Editor, data: any, item: this) => void;
        from?: CodeMirror.Pos;
    }

    export interface HintsResult {
        readonly from: Pos;
        readonly list: ReadonlyArray<Hint>
    }

    export function on(
        object: HintsResult,
        type: 'select',
        func: (completion: Hint, element: HTMLElement) => void
    ) : void;
}