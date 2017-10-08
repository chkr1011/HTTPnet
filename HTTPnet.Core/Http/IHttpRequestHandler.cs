using System.Threading.Tasks;

namespace HTTPnet.Core.Http
{
    public interface IHttpRequestHandler
    {
        Task HandleHttpRequestAsync(HttpContext httpContext);
    }
}
