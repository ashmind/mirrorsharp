import { EditorState, ChangeSpec, SelectionRange, StateCommand } from '@codemirror/next/state';
import { Line, countColumn } from '@codemirror/next/text';

// Work by Marijn Haverbeke, copied directly from next unreleased changeset:
// https://github.com/codemirror/codemirror.next/commit/315b8d7d3e5d8e5d0895a874f446d0f13da842ad#diff-b11b3c750cc62ad5d93c189071c6e321R186
// This can be removed after next CodeMirror release.

function indentString(n: number) {
    return ' '.repeat(n);
}

function changeBySelectedLine(state: EditorState, f: (line: Line, changes: Array<ChangeSpec>) => void) {
    let atLine = -1;
    return state.changeByRange(range => {
        const changes: Array<ChangeSpec> = [];
        for (let line = state.doc.lineAt(range.from);;) {
            if (line.number > atLine) {
                f(line, changes);
                atLine = line.number;
            }
            if (range.to <= line.end) break;
            line = state.doc.lineAt(line.end + 1);
        }
        const changeSet = state.changes(changes);
        return { changes,
            range: new SelectionRange(changeSet.mapPos(range.anchor, 1), changeSet.mapPos(range.head, 1)) };
    });
}

/// Add a [unit](#state.EditorState^indentUnit) of indentation to all
/// selected lines.
export const indentMore: StateCommand = ({ state, dispatch }) => {
    dispatch(state.update(changeBySelectedLine(state, (line, changes) => {
        changes.push({ from: line.start, insert: indentString(state.indentUnit) });
    })));
    return true;
};

/// Remove a [unit](#state.EditorState^indentUnit) of indentation from
/// all selected lines.
export const indentLess: StateCommand = ({ state, dispatch }) => {
    dispatch(state.update(changeBySelectedLine(state, (line, changes) => {
        const lineStart = line.slice(0, Math.min(line.length, 200));
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        const space = /^\s*/.exec(lineStart)![0];
        if (!space)
            return;

        const col = countColumn(space, 0, state.tabSize);
        const insert = indentString(Math.max(0, col - state.indentUnit));
        let keep = 0;
        while (keep < space.length && keep < insert.length && space.charCodeAt(keep) === insert.charCodeAt(keep)) keep += 1;
        changes.push({ from: line.start + keep, to: line.start + space.length, insert: insert.slice(keep) });
    })));
    return true;
};