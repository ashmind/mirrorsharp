import type { Text } from '@codemirror/state';
import type { EditorView } from '@codemirror/view';
import { lineSeparator } from '../../protocol/line-separator';

export const getString = (text: Text) => {
    // eslint-disable-next-line no-undefined
    return text.sliceString(0, undefined, lineSeparator);
};

export const getText = (view: EditorView) => {
    // eslint-disable-next-line no-undefined
    return getString(view.state.doc);
};