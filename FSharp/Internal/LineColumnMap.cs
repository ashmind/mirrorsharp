using System.Collections.Generic;

namespace MirrorSharp.FSharp.Internal {
    internal class LineColumnMap {
        private readonly IReadOnlyList<Line> _map;

        private LineColumnMap(IReadOnlyList<Line> map) {
            _map = map;
        }

        public (Line line, int column) GetLineAndColumn(int offset) {
            var map = _map;
            var line = map[0];
            for (var i = 1; i < map.Count; i++) {
                var nextLine = map[i];
                if (offset < nextLine.Start)
                    break;
                line = nextLine;
            }
            return (line, offset - line.Start);
        }

        public int GetOffset(int line, int column) {
            if (line < 1)
                return column;
            
            return _map[line - 1].Start + column;
        }

        public static LineColumnMap BuildFor(string text) {
            var map = new List<Line>();
            var start = 0;
            var previous = '\0';
            for (var i = 0; i < text.Length; i++) {
                var @char = text[i];
                if (@char == '\r' || (previous != '\r' && @char == '\n'))
                    map.Add(new Line(map.Count + 1, start, i));
                if (previous == '\n' || (previous == '\r' && @char != '\n'))
                    start = i;
                previous = @char;
            }
            map.Add(new Line(map.Count + 1, start, text.Length));
            return new LineColumnMap(map);
        }

        public struct Line {
            public Line(int number, int start, int end) {
                Number = number;
                Start = start;
                End = end;
            }

            public int Number { get; }
            public int Start { get; }
            public int End { get; }
            public int Length => End - Start;
        }
    }
}
