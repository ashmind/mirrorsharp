import type { Text, ChangeSet } from '@codemirror/state';
import { ViewPlugin, PluginValue } from '@codemirror/view';
import type { Session } from '../../session';

const sendReplace = (session: Session, from: number, to: number, text: string | Text, cursorOffset: number) => {
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    session.sendPartialText({
        start: from,
        length: to - from,
        newText: typeof text === 'string' ? text : text.toString(),
        cursorIndexAfter: cursorOffset
    });
};

const sendChanges = (session: Session, changes: ChangeSet, prevCursorOffset: number, cursorOffset: number) => {
    let changeCount = 0;
    let first: {
        from: number,
        to: number,
        text: Text
    } | undefined;
    changes.iterChanges((from, to, _f, _t, inserted) => {
        changeCount += 1;
        if (changeCount === 1) {
            first = { from, to, text: inserted };
            return;
        }

        if (changeCount === 2) {
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            sendReplace(session, first!.from, first!.to, first!.text, cursorOffset);
        }

        sendReplace(session, from, to, inserted, cursorOffset);
    });

    if (changeCount === 1) {
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        const { from, to, text } = first!;
        if (from === prevCursorOffset && to === from && text.length === 1) {
            const char = text.line(1).text.charAt(0);
            if (char === '' && text.lines === 2 && text.line(1).length === 0) {
                sendReplace(session, from, to, '\r\n', cursorOffset);
                return;
            }

            // eslint-disable-next-line @typescript-eslint/no-floating-promises, @typescript-eslint/no-non-null-assertion
            session.sendTypeChar(char);
            return;
        }

        sendReplace(session, from, to, text, cursorOffset);
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
                sendChanges(session, changes, prevCursorOffset, cursorOffset);
                return; // this will send selection move so we don't have to repeat
            }

            if (selectionSet) {
                // eslint-disable-next-line @typescript-eslint/no-floating-promises
                session.sendMoveCursor(cursorOffset);
            }
        }
    } as PluginValue;
});