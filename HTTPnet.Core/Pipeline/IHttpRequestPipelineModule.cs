using System.Threading.Tasks;

namespace HTTPnet.Pipeline
{
    public interface IHttpRequestPipelineModule
    {
        Task ProcessRequestAsync(HttpRequestPipelineModuleContext context);

        Task ProcessResponseAsync(HttpRequestPipelineModuleContext context);
    }
}
