import type { Keymap } from '@codemirror/next/keymap';
import { baseKeymap } from '@codemirror/next/commands';
import { undo, redo } from '@codemirror/next/history';

export default {
    ...baseKeymap,
    'Mod-z': undo,
    'Shift-Mod-z': redo
} as Keymap;