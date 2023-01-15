import { ViewPlugin } from '@codemirror/view';
import { getText } from './helpers/get-text';

export const notifyOnTextChanges = (onTextChange: ((getText: () => string) => void)) => ViewPlugin.define(() => ({
    update({ docChanged, view }) {
        if (!docChanged)
            return;

        onTextChange(() => getText(view));
    }
}));