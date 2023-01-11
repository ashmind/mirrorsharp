import { StreamLanguage } from '@codemirror/language';
import { vb as vbImport } from '@codemirror/legacy-modes/mode/vb';

export const vb = StreamLanguage.define(vbImport).extension;