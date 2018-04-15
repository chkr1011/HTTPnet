using System.Threading.Tasks;

namespace HTTPnet.Pipeline.Modules.Mvc
{
    public class MvcModule : IHttpRequestPipelineModule
    {
        private readonly string _relativeUri;

        public MvcModule(string relativeUri)
        {
            if (relativeUri != null) _relativeUri = relativeUri;
        }
        
        public Task ProcessRequestAsync(HttpRequestPipelineModuleContext context)
        {
            throw new System.NotImplementedException();
        }

        public Task ProcessResponseAsync(HttpRequestPipelineModuleContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
