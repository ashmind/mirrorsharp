import type { Extension } from '@codemirror/state';
import { Language, LANGUAGE_CSHARP, LANGUAGE_FSHARP, LANGUAGE_IL, LANGUAGE_PHP, LANGUAGE_VB } from '../../interfaces/protocol';
import { csharp } from './languages/csharp';
import { fsharp } from './languages/fsharp';
import { il } from './languages/il';
import { php } from './languages/php';
import { vb } from './languages/vb';

export const languageExtensions = {
    [LANGUAGE_CSHARP]: csharp,
    [LANGUAGE_VB]: vb,
    [LANGUAGE_FSHARP]: fsharp,
    [LANGUAGE_PHP]: php,
    [LANGUAGE_IL]: il
} as const;

export const switchLanguageExtension = (
    extensions: ReadonlyArray<Extension>,
    language: Language
): ReadonlyArray<Extension> => {
    const languageValues = new Set(Object.values(languageExtensions));
    const index = extensions.findIndex(e => languageValues.has(e));

    return [
        ...extensions.slice(0, index),
        languageExtensions[language],
        ...extensions.slice(index + 1, extensions.length)
    ];
};