using System.Threading.Tasks;

namespace HTTPnet.Pipeline.Modules.Rest
{
    public class RestModule : IHttpRequestPipelineModule
    {
        private readonly string _relativeUri;

        public RestModule(string relativeUri)
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

        public void RegisterController(object testController)
        {
            throw new System.NotImplementedException();
        }
    }
}
