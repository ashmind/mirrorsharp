
import { defaultKeymap, historyKeymap, indentWithTab } from '@codemirror/commands';
import { keymap } from '@codemirror/view';

export const keymaps = keymap.of([
    ...defaultKeymap,
    ...historyKeymap,
    indentWithTab
]);