import { ViewPlugin } from '@codemirror/view';

export const notifyOnTextChanges = (onTextChange: (() => void)) => ViewPlugin.define(() => ({
    update({ docChanged }) {
        if (!docChanged)
            return;

        onTextChange();
    }
}));