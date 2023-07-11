import { StreamLanguage } from '@codemirror/language';
import { vb as vbImport } from '@codemirror/legacy-modes/mode/vb';

// Test-only:
// ts-unused-exports:disable-next-line
export const vbLanguage = StreamLanguage.define(vbImport);

export const vb = vbLanguage.extension;