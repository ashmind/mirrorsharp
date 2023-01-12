import { StreamLanguage } from '@codemirror/language';
import { vb as vbImport } from '@codemirror/legacy-modes/mode/vb';

export const vbLanguage = StreamLanguage.define(vbImport);
export const vb = vbLanguage.extension;