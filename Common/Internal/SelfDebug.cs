using System;
using System.Collections.Generic;
using Nito;

namespace MirrorSharp.Internal {
    internal class SelfDebug {
        private readonly Deque<LogEntry> _log = new Deque<LogEntry>();

        public void Log(string eventType, string? message, int cursorPosition, string text) {
            _log.AddToBack(new LogEntry(DateTimeOffset.Now, eventType, message, cursorPosition, text));
            while (_log.Count > 100) {
                _log.RemoveFromFront();
            }
        }

        public IEnumerable<LogEntry> GetLogEntries() => _log;

        public struct LogEntry {
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
