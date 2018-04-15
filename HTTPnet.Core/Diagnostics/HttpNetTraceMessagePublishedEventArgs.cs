using System;

namespace HTTPnet.Diagnostics
{
    public sealed class HttpNetTraceMessagePublishedEventArgs : EventArgs
    {
        public HttpNetTraceMessagePublishedEventArgs(int threadId, string source, HttpNetTraceLevel level, string message, Exception exception)
        {
            ThreadId = threadId;
            Source = source;
            Level = level;
            Message = message;
            Exception = exception;
        }

        public int ThreadId { get; }

        public string Source { get; }

        public HttpNetTraceLevel Level { get; }

        public string Message { get; }

        public Exception Exception { get; }
    }
}
