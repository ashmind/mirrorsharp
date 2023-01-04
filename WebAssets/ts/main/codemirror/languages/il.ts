import { StreamLanguage } from '@codemirror/language';
// Temporary stub
import { gas } from '@codemirror/legacy-modes/mode/gas';

export const il = StreamLanguage.define(gas).extension;