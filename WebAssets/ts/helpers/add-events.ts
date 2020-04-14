import type * as CodeMirror from 'codemirror';
import type {
    Connection,
    ConnectionOpenHandler,
    ConnectionMessageHandler,
    ConnectionErrorHandler,
    ConnectionCloseHandler
} from '../interfaces/connection';

// Workaround for https://github.com/microsoft/TypeScript/issues/37204
// and https://github.com/microsoft/TypeScript/issues/37263
type KnownEventGroups<TExtensionServerOptions, TSlowUpdateExtensionData> =
    // CodeMirror
    [CodeMirror.Editor, {
        beforeChange?: (instance: CodeMirror.Editor, changeObj: CodeMirror.EditorChangeCancellable) => void;
        cursorActivity?: (instance: CodeMirror.Editor) => void;
        changes?: (instance: CodeMirror.Editor, changes: Array<CodeMirror.EditorChangeLinkedList>) => void;
        keypress?: (instance: CodeMirror.Editor, event: KeyboardEvent) => void;
        endCompletion?: (instance: CodeMirror.Editor) => void;
    }]
    |
    // Connection
    [Connection<TExtensionServerOptions, TSlowUpdateExtensionData>, {
        open?: ConnectionOpenHandler;
        message?: ConnectionMessageHandler<TExtensionServerOptions, TSlowUpdateExtensionData>;
        error?: ConnectionErrorHandler;
        close?: ConnectionCloseHandler;
    }];

function addEvents<TExtensionServerOptions, TSlowUpdateExtensionData>(
    ...args: KnownEventGroups<TExtensionServerOptions, TSlowUpdateExtensionData>
) {
    const [target, handlers] = args;
    for (const key in handlers) {
        (target.on as Function)(key, handlers[key as keyof typeof handlers]);
    }

    return (): void => {
        for (const key in handlers) {
            (target.off as Function)(key, handlers[key as keyof typeof handlers]);
        }
    };
}

export { addEvents };