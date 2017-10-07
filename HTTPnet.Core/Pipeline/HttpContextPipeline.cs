using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HTTPnet.Core.Http;

namespace HTTPnet.Core.Pipeline
{
    public class HttpContextPipeline : IHttpRequestHandler, IHttpContextPipelineHandler
    {
        private readonly List<IHttpContextPipelineHandler> _processors = new List<IHttpContextPipelineHandler>();

        public Task HandleUnhandledExceptionAsync(HttpContext httpContext, Exception exception)
        {
            return Task.FromResult(0);
        }

        public async Task HandleHttpRequestAsync(HttpContext httpConext)
        {
            try
            {
                var pipelineContext = new HttpContextPipelineHandlerContext(httpConext);

                var offset = -1;
                foreach (var processor in _processors)
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
                    await _processors[i].ProcessResponseAsync(pipelineContext);
                    if (pipelineContext.BreakPipeline)
                    {
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                await HandleUnhandledExceptionAsync(httpConext, exception);
            }
        }

        public void Add(IHttpContextPipelineHandler processor)
        {
            if (processor == null) throw new ArgumentNullException(nameof(processor));

            _processors.Add(processor);
        }

        public void Insert(int index, IHttpContextPipelineHandler processor)
        {
            if (processor == null) throw new ArgumentNullException(nameof(processor));

            _processors.Insert(index, processor);
        }

        public void InsertAfter<TBefore>(IHttpContextPipelineHandler processor) where TBefore : IHttpContextPipelineHandler
        {
            if (processor == null) throw new ArgumentNullException(nameof(processor));

            
        }

        public void InsertBefore<TAfter>(IHttpContextPipelineHandler processor) where TAfter : IHttpContextPipelineHandler
        {
            
        }

        public void Clear()
        {
            _processors.Clear();
        }

        public async Task ProcessRequestAsync(HttpContextPipelineHandlerContext context)
        {
            ////foreach (var processor in _processors)
            ////{
            ////    await processor.ProcessRequestAsync(pipelineContext);
            ////    offset++;

            ////    if (pipelineContext.BreakPipeline)
            ////    {
            ////        break;
            ////    }
            ////}
        }

        public async Task ProcessResponseAsync(HttpContextPipelineHandlerContext context)
        {
            
        }
    }
}
