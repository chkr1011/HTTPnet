using System;

namespace HTTPnet.Core.Diagnostics
{
    public static class HttpNetTrace
    {
        public static event EventHandler<HttpNetTraceMessagePublishedEventArgs> TraceMessagePublished;

        public static void Verbose(string source, string message, params object[] parameters)
        {
            Publish(source, HttpNetTraceLevel.Verbose, null, message, parameters);
        }

        public static void Information(string source, string message, params object[] parameters)
        {
            Publish(source, HttpNetTraceLevel.Information, null, message, parameters);
        }

        public static void Warning(string source, string message, params object[] parameters)
        {
            Publish(source, HttpNetTraceLevel.Warning, null, message, parameters);
        }

        public static void Warning(string source, Exception exception, string message, params object[] parameters)
        {
            Publish(source, HttpNetTraceLevel.Warning, exception, message, parameters);
        }

        public static void Error(string source, string message, params object[] parameters)
        {
            Publish(source, HttpNetTraceLevel.Error, null, message, parameters);
        }

        public static void Error(string source, Exception exception, string message, params object[] parameters)
        {
            Publish(source, HttpNetTraceLevel.Error, exception, message, parameters);
        }

        private static void Publish(string source, HttpNetTraceLevel traceLevel, Exception exception, string message, params object[] parameters)
        {
            var handler = TraceMessagePublished;
            if (handler == null)
            {
                return;
            }

            if (parameters?.Length > 0)
            {
                try
                {
                    message = string.Format(message, parameters);
                }
                catch (Exception formatException)
                {
                    Error(nameof(HttpNetTrace), formatException, "Error while tracing message: " + message);
                    return;
                }
            }

            handler.Invoke(null, new HttpNetTraceMessagePublishedEventArgs(Environment.CurrentManagedThreadId, source, traceLevel, message, exception));
        }
    }
}
