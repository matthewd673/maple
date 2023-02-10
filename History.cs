using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace maple
{

    public enum HistoryEventType
    {
        Add,
        Remove,
        AddLine,
        RemoveLine,
        AddSelection,
        RemoveSelection,
        IndentLine,
        DeindentLine,
    }

    public class History
    {
        private List<HistoryEvent> events = new();
        private List<HistoryEvent> redoEvents = new();

        private static ReaderWriterLock rwLock = new ReaderWriterLock();
        private const int WriterLockTimeout = 500;

        public History() { }

        public void PushEvent(HistoryEvent e)
        {
            events.Add(e);
            redoEvents.Clear();
        }

        public HistoryEvent PopEvent()
        {
            HistoryEvent e = events[^1];
            events.RemoveAt(events.Count - 1);

            redoEvents.Add(new HistoryEvent(
                e.EventType,
                e.TextDelta,
                e.DeltaPos,
                new Point(Editor.DocCursor.DX, Editor.DocCursor.DY),
                e.SelectionPoints,
                e.Combined
            ));

            return e;
        }

        public bool HasNext()
        {
            return events.Count > 0;
        }

        public HistoryEvent PopRedoEvent()
        {
            HistoryEvent e = redoEvents[^1];
            redoEvents.RemoveAt(redoEvents.Count - 1);

            events.Add(new HistoryEvent(
                e.EventType,
                e.TextDelta,
                e.DeltaPos,
                new Point(Editor.DocCursor.DX, Editor.DocCursor.DY),
                e.SelectionPoints,
                e.Combined
            ));

            return e;
        }

        public bool HasNextRedo()
        {
            return redoEvents.Count > 0;
        }

        public bool NextRedoCombined()
        {
            return redoEvents.Count > 0 && redoEvents[^1].Combined;
        }

        public void Clear()
        {
            events.Clear();
            redoEvents.Clear();
        }

    }

    public struct HistoryEvent
    {
        public HistoryEventType EventType { get; set; }
        public string TextDelta { get; set; }
        public Point DeltaPos { get; set; }
        public Point CursorPos { get; set; }
        public Point[] SelectionPoints { get; set; }
        public bool Combined { get; set; }

        public HistoryEvent(HistoryEventType eventType, string textDelta, Point deltaPos, Point cursorPos, Point[] selectionPoints = null, bool combined = false)
        {
            EventType = eventType;
            TextDelta = textDelta;
            DeltaPos = deltaPos;
            CursorPos = cursorPos;
            SelectionPoints = selectionPoints;
            Combined = combined;
        }
    }

}