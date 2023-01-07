// import type * as CodeMirror from 'codemirror';
import type { Connection, ConnectionEventMap } from '../main/connection';

// Workaround for https://github.com/microsoft/TypeScript/issues/37204
// and https://github.com/microsoft/TypeScript/issues/37263
type KnownEventGroups<TExtensionServerOptions, TSlowUpdateExtensionData> =
    // CodeMirror
    // [CodeMirror.Editor, {
    //     beforeChange?: (instance: CodeMirror.Editor, changeObj: CodeMirror.EditorChangeCancellable) => void;
    //     cursorActivity?: (instance: CodeMirror.Editor) => void;
    //     changes?: (instance: CodeMirror.Editor, changes: Array<CodeMirror.EditorChangeLinkedList>) => void;
    //     keypress?: (instance: CodeMirror.Editor, event: KeyboardEvent) => void;
    //     endCompletion?: (instance: CodeMirror.Editor) => void;
    // }]
    // |
    // Connection
    [
        Connection<TExtensionServerOptions, TSlowUpdateExtensionData>,
        Partial<ConnectionEventMap<TExtensionServerOptions, TSlowUpdateExtensionData>>
    ];

// eslint-disable-next-line @typescript-eslint/no-explicit-any
type OnOffFunction = (key: string, handler: ((...args: Array<any>) => void)) => void;

export const addEvents = <TExtensionServerOptions, TSlowUpdateExtensionData>(
    ...args: KnownEventGroups<TExtensionServerOptions, TSlowUpdateExtensionData>
) => {
    const [target, handlers] = args;
    for (const key in handlers) {
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        (target.on as OnOffFunction)(key, handlers[key as keyof typeof handlers]!);
    }

    return (): void => {
        for (const key in handlers) {
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            (target.off as OnOffFunction)(key, handlers[key as keyof typeof handlers]!);
        }
    };
};