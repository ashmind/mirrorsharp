
import { defaultKeymap, indentMore, indentLess } from '@codemirror/next/commands';
import { historyKeymap } from '@codemirror/next/history';
import { keymap, ViewPlugin, EditorView } from '@codemirror/next/view';

const tabTrapped = new WeakMap<EditorView, boolean>();
const tabTrapPlugin = ViewPlugin.define(view => {
    return ({
        update() {
            if (view.hasFocus)
                tabTrapped.set(view, true);
        }
    });
});

export default [tabTrapPlugin, keymap([
    ...defaultKeymap,
    ...historyKeymap,
    { key: 'Tab', run: view => tabTrapped.get(view) ? indentMore(view) : false },
    { key: 'Shift-Tab', run: view => tabTrapped.get(view) ? indentLess(view) : false },
    { key: 'Escape', run: view => {
        tabTrapped.set(view, false);
        return true;
    } }
])];