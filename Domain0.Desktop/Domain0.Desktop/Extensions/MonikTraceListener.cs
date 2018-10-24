using Monik.Common;
using System;
using System.Diagnostics;

namespace Domain0.Desktop.Extensions
{
    public class MonikTraceListener : TraceListener
    {
        private readonly IMonik _monik;

        public MonikTraceListener(IMonik monik)
        {
            _monik = monik;
        }

        public override void Write(string message)
        {
        }

        public override void WriteLine(string message)
        {
        }

        public override void Flush()
        {
            base.Flush();
            _monik.OnStop();
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
            string format, params object[] args)
        {
            base.TraceEvent(eventCache, source, eventType, id, format, args);
            ToMonik(eventType, format, args);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
            string message)
        {
            base.TraceEvent(eventCache, source, eventType, id, message);
            ToMonik(eventType, message);
        }

        private void ToMonik(TraceEventType eventType, string format, object[] args)
        {
            switch (eventType)
            {
                case TraceEventType.Critical:
                    _monik.ApplicationFatal(format, args);
                    break;
                case TraceEventType.Error:
                    _monik.ApplicationError(format, args);
                    break;
                case TraceEventType.Warning:
                    _monik.ApplicationWarning(format, args);
                    break;
                case TraceEventType.Information:
                    _monik.ApplicationInfo(format, args);
                    break;
            }
        }

        private void ToMonik(TraceEventType eventType, string format)
        {
            switch (eventType)
            {
                case TraceEventType.Critical:
                    _monik.ApplicationFatal(format, Array.Empty<object>());
                    break;
                case TraceEventType.Error:
                    _monik.ApplicationError(format, Array.Empty<object>());
                    break;
                case TraceEventType.Warning:
                    _monik.ApplicationWarning(format, Array.Empty<object>());
                    break;
                case TraceEventType.Information:
                    _monik.ApplicationInfo(format, Array.Empty<object>());
                    break;
            }
        }
    }
}