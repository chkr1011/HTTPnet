using System;
using System.Collections.Generic;

namespace HTTPnet.Core.Http
{
    public static class HttpHeaderExtensions
    {
        public static bool ConnectionMustBeClosed(this Dictionary<string, string> headers)
        {
            return headers.ValueEquals(HttpHeader.Connection, "Close");
        }

        public static bool RequiresContinue(this Dictionary<string, string> headers)
        {
            return headers.TryGetValue(HttpHeader.Expect, out var value) && string.Equals(value, "100-Continue", StringComparison.OrdinalIgnoreCase);
        }

        public static bool ValueEquals(this Dictionary<string, string> headers, string headerName, string expectedValue)
        {
            if (headers == null) throw new ArgumentNullException(nameof(headers));
            if (headerName == null) throw new ArgumentNullException(nameof(headerName));
            if (expectedValue == null) throw new ArgumentNullException(nameof(expectedValue));

            if (!headers.TryGetValue(headerName, out var value))
            {
                return false;
            }

            return string.Equals(value, expectedValue, StringComparison.OrdinalIgnoreCase);
        }
    }
}
