import { LANGUAGE_CSHARP, LANGUAGE_FSHARP, LANGUAGE_IL, LANGUAGE_PHP, LANGUAGE_VB } from '../protocol/languages';
import { cil } from './languages/cil';
import { csharp } from './languages/csharp';
import { fsharp } from './languages/fsharp';
import { php } from './languages/php';
import { vb } from './languages/vb';

export const languageExtensions = {
    [LANGUAGE_CSHARP]: csharp,
    [LANGUAGE_VB]: vb,
    [LANGUAGE_FSHARP]: fsharp,
    [LANGUAGE_PHP]: php,
    [LANGUAGE_IL]: cil
} as const;