import type { Extension } from '@codemirror/state';

export const switchableExtension = <T>(initialState: T, getExtension: (state: T) => Extension) => {
    let extension = getExtension(initialState);

    return {
        get extension() { return extension; },

        switch: (extensions: ReadonlyArray<Extension>, newState: T) => {
            const index = extensions.indexOf(extension);
            extension = getExtension(newState);
            return [
                ...extensions.slice(0, index),
                extension,
                ...extensions.slice(index + 1, extensions.length)
            ] as const;
        }
    } as const;
};

export type SwitchExtension<T> = ReturnType<typeof switchableExtension<T>>['switch'];