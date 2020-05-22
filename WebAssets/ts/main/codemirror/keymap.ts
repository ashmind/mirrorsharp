import { keymap } from '@codemirror/next/keymap';
import { baseKeymap } from '@codemirror/next/commands';
import { undo, redo } from '@codemirror/next/history';
import { indentMore, indentLess } from './indent-temp';
import { ViewPlugin, EditorView } from '@codemirror/next/view';

const tabTrapped = new WeakMap<EditorView, boolean>();
const tabTrapPlugin = ViewPlugin.define(view => {
    return ({
        update() {
            if (view.hasFocus)
                tabTrapped.set(view, true);
        }
    });
});

export default [tabTrapPlugin, keymap({
    ...baseKeymap,

    'Mod-z': undo,
    'Shift-Mod-z': redo,

    'Tab': view => tabTrapped.get(view) ? indentMore(view) : false,
    'Shift-Tab': view => tabTrapped.get(view) ? indentLess(view) : false,
    'Escape': view => {
        tabTrapped.set(view, false);
        return true;
    }
})];