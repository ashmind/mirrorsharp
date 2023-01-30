import type { Text, ChangeSet, EditorState } from '@codemirror/state';
import { ViewPlugin, PluginValue } from '@codemirror/view';
import { lineSeparator } from '../../protocol/line-separator';
import type { Session } from '../../protocol/session';
import { convertToServerPosition } from '../helpers/convert-position';

const sendReplace = (session: Session, doc: Text, from: number, to: number, newText: string | Text, cursorOffset: number) => {
    const start = convertToServerPosition(doc, from);
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    session.sendPartialText({
        start,
        length: convertToServerPosition(doc, to) - start,
        newText: typeof newText === 'string' ? newText : newText.toString(),
        cursorIndexAfter: convertToServerPosition(doc, cursorOffset)
    });
};

const sendChanges = (session: Session, startState: EditorState, changes: ChangeSet, prevCursorOffset: number, cursorOffset: number) => {
    let changeCount = 0;
    let single: {
        from: number,
        to: number,
        text: Text
    } | undefined | null;
    let startOffset = 0;
    let lastDoc = startState.doc;
    changes.iterChanges((from, to, _f, _t, inserted) => {
        changeCount += 1;
        if (changeCount === 1) {
            single = { from, to, text: inserted };
            return;
        }

        if (single) {
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            sendReplace(session, lastDoc, single.from, single.to, single.text, cursorOffset);
            lastDoc = lastDoc.replace(from, to, inserted);
            startOffset += single.text.length - (single.to - single.from);
            single = null;
        }

        sendReplace(session, lastDoc, startOffset + from, startOffset + to, inserted, cursorOffset);
        startOffset += inserted.length - (to - from);
        lastDoc = lastDoc.replace(from, to, inserted);
    });

    if (single) {
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        const { from, to, text } = single;
        if (from === prevCursorOffset && to === from && text.length === 1) {
            const char = text.line(1).text.charAt(0);
            if (char === '' && text.lines === 2 && text.line(1).length === 0) {
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                sendReplace(session, startState.doc, from, to, lineSeparator, cursorOffset);
                return;
            }

            // eslint-disable-next-line @typescript-eslint/no-floating-promises, @typescript-eslint/no-non-null-assertion
            session.sendTypeChar(char);
            return;
        }

        sendReplace(session, startState.doc, from, to, text, cursorOffset);
    }
};

export const sendChangesToServer = (session: Session) => ViewPlugin.define(view => {
    session.setFullText({
        getText: () => view.state.doc.toString(),
        getCursorIndex: () => view.state.selection.main.from
    });

    return {
        update({ docChanged, selectionSet, changes, state, startState }) {
            const prevCursorOffset = startState.selection.main.from;
            const cursorOffset = state.selection.main.from;

            if (docChanged) {
                sendChanges(session, startState, changes, prevCursorOffset, cursorOffset);
                return; // this will send selection move so we don't have to repeat
            }

            if (selectionSet) {
                // eslint-disable-next-line @typescript-eslint/no-floating-promises
                session.sendMoveCursor(convertToServerPosition(startState.doc, cursorOffset));
            }
        }
    } as PluginValue;
});