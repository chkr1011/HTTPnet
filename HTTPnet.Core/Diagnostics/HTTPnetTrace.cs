using System;
using System.Linq;

namespace HTTPnet.Core.Diagnostics
{
    public static class HttpNetTrace
    {
        public static event EventHandler<string> TraceMessagePublished;

        public static void Verbose(string message, params object[] parameters)
        {
            var handler = TraceMessagePublished;
            if (handler == null)
            {
                return;
            }

            if (parameters.Any())
            {
                message = string.Format(message, parameters);
            }

            handler(null, message);
        }
    }
}
