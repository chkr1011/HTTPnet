using System;
using System.Threading.Tasks;

namespace HTTPnet.Core.Http
{
    public interface IHttpRequestHandler
    {
        Task HandleUnhandledExceptionAsync(HttpContext httpContext, Exception exception);

        Task HandleHttpRequestAsync(HttpContext httpConext);
    }
}