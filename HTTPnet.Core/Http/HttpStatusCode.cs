namespace HTTPnet.Core.Http
{
    public enum HttpStatusCode
    {
        Continue = 100,
        SwitchingProtocols = 101,

        OK = 200,

        NotModified = 304,

        NotFound = 404,

        InternalServerError = 500,
        HttpVersionNotSupported = 505,
    }
}
