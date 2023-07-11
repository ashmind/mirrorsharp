import { Text } from '@codemirror/state';
import type { ServerPosition } from '../../protocol/messages';
import { convertFromServerPosition, convertToServerPosition } from './convert-position';

const cases = [
    [['a'], 0, 0],
    [['a'], 1, 1],
    [['a', 'b'], 1, 1],
    [['a', 'b'], 2, 3],
    [['ab'], 2, 2],
    [['a', 'b', 'c'], 5, 7]
] as ReadonlyArray<[lines: ReadonlyArray<string>, cmPosition: number, serverPosition: number]>;

test.each(cases)('convertToServerPosition(%p, %p) returns %p', (lines, cmPosition, expectedServerPosition) => {
    expect(convertToServerPosition(Text.of(lines), cmPosition)).toEqual(expectedServerPosition);
});

const invertedCases = cases.map(([lines, cmPosition, serverPosition]) => [lines, serverPosition, cmPosition] as const);
test.each(invertedCases)('convertFromServerPosition(%p, %p) returns %p', (lines, serverPosition, expectedCMPosition) => {
    expect(convertFromServerPosition(Text.of(lines), serverPosition as unknown as ServerPosition))
        .toEqual(expectedCMPosition);
});