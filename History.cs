using System;
using System.Collections.Generic;

namespace maple
{

    public enum HistoryEventType
    {
        Add,
        Remove,
    }

    public class History
    {
        private List<HistoryEvent> events = new();
        private List<HistoryEvent> redoEvents = new();

        public History() { }

        public void PushEvent(HistoryEvent e)
        {
            events.Add(e);
        }

        public HistoryEvent PopEvent()
        {
            HistoryEvent e = events[^1];
            events.RemoveAt(events.Count - 1);
            redoEvents.Add(e);

            return e;
        }

        public HistoryEvent PopRedoEvent()
        {
            HistoryEvent e = redoEvents[^1];
            redoEvents.RemoveAt(redoEvents.Count - 1);

            return e;
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
        public string DeltaPos { get; set; }

        public HistoryEvent(HistoryEventType eventType, string textDelta, string deltaPos)
        {
            EventType = eventType;
            TextDelta = textDelta;
            DeltaPos = deltaPos;
        }
    }

}