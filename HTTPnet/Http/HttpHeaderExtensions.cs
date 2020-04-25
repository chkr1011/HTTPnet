using System;
using System.Collections.Generic;
using HTTPnet.Exceptions;

namespace HTTPnet.Http
{
    public static class HttpHeaderExtensions
    {
        public static long GetContentLength(this Dictionary<string, string> headers)
        {
            var length = 0L;

            if (headers.TryGetValue(HttpHeader.ContentLength, out var value))
            {
                if (!long.TryParse(value, out length))
                {
                    throw new HttpRequestInvalidException();
                }
            }

            return length;
        }

        public static bool HasConnectionKeepAlive(this Dictionary<string, string> headers)
        {
            return headers.ValueEquals(HttpHeader.Connection, "Keep-Alive");
        }

        public static bool HasExpectsContinue(this Dictionary<string, string> headers)
        {
            return headers.ValueEquals(HttpHeader.Expect, "100-Continue");
        }

        public static bool SupportsGzipCompression(this Dictionary<string, string> headers)
        {
            return headers.ValueContains(HttpHeader.AcceptEncoding, "gzip");
        }

        public static bool ValueContains(this Dictionary<string, string> headers, string headerName, string expectedValue)
        {
            if (headers == null) throw new ArgumentNullException(nameof(headers));
            if (headerName == null) throw new ArgumentNullException(nameof(headerName));
            if (expectedValue == null) throw new ArgumentNullException(nameof(expectedValue));

            if (!headers.TryGetValue(headerName, out var value))
            {
                return false;
            }

            return value.IndexOf(expectedValue, StringComparison.OrdinalIgnoreCase) > -1;
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
