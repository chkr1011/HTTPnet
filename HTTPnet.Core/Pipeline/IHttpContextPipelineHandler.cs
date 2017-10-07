using System.Threading.Tasks;

namespace HTTPnet.Core.Pipeline
{
    public interface IHttpContextPipelineHandler
    {
        Task ProcessRequestAsync(HttpContextPipelineHandlerContext context);

        Task ProcessResponseAsync(HttpContextPipelineHandlerContext context);
    }
}
