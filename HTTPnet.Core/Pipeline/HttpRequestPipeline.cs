using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HTTPnet.Http;

namespace HTTPnet.Pipeline
{
    public class HttpRequestPipeline : IHttpRequestHandler
    {
        private readonly List<IHttpRequestPipelineModule> _handlers = new List<IHttpRequestPipelineModule>();
        private readonly IHttpRequestPipelineExceptionHandler _exceptionHandler;

        public HttpRequestPipeline(IHttpRequestPipelineExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }
        
        public async Task HandleHttpRequestAsync(HttpContext httpContext)
        {
            var pipelineContext = new HttpRequestPipelineModuleContext(httpContext);
            var offset = -1;

            try
            {
                foreach (var processor in _handlers)
                {
                    await processor.ProcessRequestAsync(pipelineContext);
                    offset++;

                    if (pipelineContext.BreakPipeline)
                    {
                        break;
                    }
                }

                pipelineContext.BreakPipeline = false;

                for (var i = offset; i >= 0; i--)
                {
                    await _handlers[i].ProcessResponseAsync(pipelineContext).ConfigureAwait(false);
                    if (pipelineContext.BreakPipeline)
                    {
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                await _exceptionHandler.HandleExceptionAsync(httpContext, exception);
            }
        }

        public void Add(IHttpRequestPipelineModule processor)
        {
            if (processor == null) throw new ArgumentNullException(nameof(processor));

            _handlers.Add(processor);
        }

        public void Insert(int index, IHttpRequestPipelineModule processor)
        {
            if (processor == null) throw new ArgumentNullException(nameof(processor));

            _handlers.Insert(index, processor);
        }

        public void InsertAfter<TBefore>(IHttpRequestPipelineModule processor) where TBefore : IHttpRequestPipelineModule
        {
            if (processor == null) throw new ArgumentNullException(nameof(processor));

            Insert(_handlers.FindIndex(h => h is TBefore) + 1, processor);
        }

        public void InsertBefore<TAfter>(IHttpRequestPipelineModule processor) where TAfter : IHttpRequestPipelineModule
        {
            Insert(_handlers.FindIndex(h => h is TAfter), processor);
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
