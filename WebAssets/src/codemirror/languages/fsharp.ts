import { StreamLanguage } from '@codemirror/language';
import { fSharp } from '@codemirror/legacy-modes/mode/mllike';

export const fsharp = StreamLanguage.define(fSharp).extension;