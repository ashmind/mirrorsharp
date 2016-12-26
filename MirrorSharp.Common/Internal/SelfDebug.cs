using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using Nito;

namespace MirrorSharp.Internal {
    public class SelfDebug {
        private readonly Deque<LogEntry> _log = new Deque<LogEntry>();

        public void Log(string eventType, string message, int cursorPosition, SourceText sourceText) {
            _log.AddToBack(new LogEntry(DateTimeOffset.Now, eventType, message, cursorPosition, sourceText));
            while (_log.Count > 100) {
                _log.RemoveFromFront();
            }
        }

        public IEnumerable<LogEntry> GetLogEntries() => _log;

        public struct LogEntry {
            public DateTimeOffset DateTime { get; }
            public string EventType { get; }
            public string Message { get; }
            public int CursorPosition { get; }
            public SourceText SourceText { get; }

            public LogEntry(DateTimeOffset dateTime, string eventType, string message, int cursorPosition, SourceText sourceText) {
                DateTime = dateTime;
                EventType = eventType;
                Message = message;
                CursorPosition = cursorPosition;
                SourceText = sourceText;
            }
        }
    }
}
