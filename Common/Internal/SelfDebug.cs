using System;
using System.Collections.Generic;

namespace MirrorSharp.Internal {
    internal class SelfDebug {
        private readonly LogEntry[] _log = new LogEntry[100];
        private int _logIndex = -1;
        private bool _endReached;

        public void Log(string eventType, string? message, int cursorPosition, string text) {
            _logIndex += 1;
            if (_logIndex >= _log.Length) {
                _logIndex = 0;
                _endReached = true;
            }

            _log[_logIndex] = new(DateTimeOffset.Now, eventType, message, cursorPosition, text);
        }

        public IEnumerable<LogEntry> GetLogEntries() {
            if (_endReached) {
                for (var i = _logIndex + 1; i < _log.Length; i++) {
                    yield return _log[i];
                }
            }

            for (var i = 0; i <= _logIndex; i++) {
                yield return _log[i];
            }
        }

        public readonly struct LogEntry {
            public DateTimeOffset DateTime { get; }
            public string EventType { get; }
            public string? Message { get; }
            public int CursorPosition { get; }
            public string Text { get; }

            public LogEntry(DateTimeOffset dateTime, string eventType, string? message, int cursorPosition, string text) {
                DateTime = dateTime;
                EventType = eventType;
                Message = message;
                CursorPosition = cursorPosition;
                Text = text;
            }
        }
    }
}
