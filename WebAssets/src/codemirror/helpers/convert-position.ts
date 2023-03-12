import type { Text } from '@codemirror/state';
import { lineSeparator } from '../../protocol/line-separator';
import type { ServerPosition } from '../../protocol/messages';

export const getEnd = (start: ServerPosition, length: number) => ((start as unknown as number) + length) as unknown as ServerPosition;
export const getLength = (start: ServerPosition, end: ServerPosition) => (end as unknown as number) - (start as unknown as number);

export const convertToServerPosition = (doc: Text, position: number): ServerPosition => {
    const line = doc.lineAt(position);
    const lineSeparatorOffset = (line.number - 1) * (lineSeparator.length - 1);
    return (position + lineSeparatorOffset) as unknown as ServerPosition;
};

export const convertFromServerPosition = (doc: Text, position: ServerPosition) => {
    const positionAsNumber = position as unknown as number;
    let serverLineStart = 0;
    for (let n = 1; n <= doc.lines; n++) {
        const line = doc.line(n);
        const nextServerLineStart = serverLineStart + line.length + lineSeparator.length;
        if (positionAsNumber < nextServerLineStart || n === doc.lines)
            return (positionAsNumber - serverLineStart) + line.from;
        serverLineStart = nextServerLineStart;
    }

    throw new Error(`Failed to map position from server: ${positionAsNumber}`);
};