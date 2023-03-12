export const LANGUAGE_CSHARP = 'C#';
export const LANGUAGE_VB = 'Visual Basic';
export const LANGUAGE_FSHARP = 'F#';
export const LANGUAGE_PHP = 'PHP';
export const LANGUAGE_IL = 'IL';

export const LANGUAGE_DEFAULT = LANGUAGE_CSHARP;

export type Language = typeof LANGUAGE_CSHARP
    | typeof LANGUAGE_VB
    | typeof LANGUAGE_FSHARP
    | typeof LANGUAGE_PHP
    | typeof LANGUAGE_IL;