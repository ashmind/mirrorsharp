import type { Text } from '@codemirror/state';
import { lineSeparator } from '../../protocol/line-separator';

export const convertToServerPosition = (doc: Text, position: number) => {
    const line = doc.lineAt(position);
    const lineSeparatorOffset = (line.number - 1) * (lineSeparator.length - 1);
    return position + lineSeparatorOffset;
};

export const convertFromServerPosition = (doc: Text, position: number) => {
    let serverLineStart = 0;
    for (let n = 1; n <= doc.lines; n++) {
        const line = doc.line(n);
        const nextServerLineStart = serverLineStart + line.length + lineSeparator.length;
        if (position < nextServerLineStart || n === doc.lines)
            return (position - serverLineStart) + line.from;
        serverLineStart = nextServerLineStart;
    }

    throw new Error(`Failed to map position from server: ${position}`);
};