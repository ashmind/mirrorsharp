import type { Text, ChangeSet } from '@codemirror/state';
import { ViewPlugin, PluginValue } from '@codemirror/view';
import { addEvents } from '../../../helpers/add-events';
import type { Connection } from '../../connection';

function sendReplace<O, U>(connection: Connection<O, U>, from: number, to: number, text: Text, cursorOffset: number) {
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    connection.sendReplaceText(from, to - from, text.toString(), cursorOffset);
}

function sendChanges<O, U>(connection: Connection<O, U>, changes: ChangeSet, prevCursorOffset: number, cursorOffset: number) {
    let changeCount = 0;
    let firstFrom: number|undefined;
    let firstTo: number|undefined;
    let firstText: Text|undefined;
    changes.iterChanges((from, to, _f, _t, inserted) => {
        changeCount += 1;
        if (changeCount === 1) {
            firstFrom = from;
            firstTo = to;
            firstText = inserted;
            return;
        }

        if (changeCount === 2) {
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            sendReplace(connection, firstFrom!, firstTo!, firstText!, cursorOffset);
        }

        sendReplace(connection, from, to, inserted, cursorOffset);
    });

    if (changeCount === 1) {
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        if (firstFrom === prevCursorOffset && firstTo === firstFrom && firstText!.length === 1) {
            // eslint-disable-next-line @typescript-eslint/no-floating-promises, @typescript-eslint/no-non-null-assertion
            connection.sendTypeChar(firstText!.line(1).text.charAt(0));
            return true;
        }

        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        sendReplace(connection, firstFrom!, firstTo!, firstText!, cursorOffset);
    }
}

export const sendChangesToServer = <O, U>(connection: Connection<O, U>) => ViewPlugin.define(view => {
    const sendFullText = () => {
        if (view.state.doc.length === 0)
            return;
        sendReplace(connection, 0, 0, view.state.doc, view.state.selection.main.from);
    };

    if (connection.isOpen())
        sendFullText();

    const removeEvents = addEvents(connection, { open: sendFullText });
    return {
        update({ docChanged, selectionSet, changes, state, startState }) {
            const prevCursorOffset = startState.selection.main.from;
            const cursorOffset = state.selection.main.from;

            if (docChanged) {
                sendChanges(connection, changes, prevCursorOffset, cursorOffset);
                return; // this will send selection move so we don't have to repeat
            }

            if (selectionSet) {
                // eslint-disable-next-line @typescript-eslint/no-floating-promises
                connection.sendMoveCursor(cursorOffset);
            }
        },

        destroy: removeEvents
    } as PluginValue;
});