using System.Threading.Tasks;

namespace HTTPnet.Http
{
    public interface IHttpRequestHandler
    {
        Task HandleHttpRequestAsync(HttpContext httpContext);
    }
}
