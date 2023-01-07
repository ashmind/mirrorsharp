
import { defaultKeymap, historyKeymap, indentWithTab } from '@codemirror/commands';
import { keymap } from '@codemirror/view';

export default [keymap.of([
    ...defaultKeymap,
    ...historyKeymap,
    indentWithTab
])];