using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HTTPnet.Core.Http;

namespace HTTPnet.Core.Pipeline
{
    public class HttpContextPipeline : IHttpRequestHandler
    {
        private readonly List<IHttpContextPipelineHandler> _handlers = new List<IHttpContextPipelineHandler>();
        private readonly IHttpContextPipelineExceptionHandler _exceptionHandler;

        public HttpContextPipeline(IHttpContextPipelineExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        public Task HandleUnhandledExceptionAsync(HttpContext httpContext, Exception exception)
        {
            return _exceptionHandler.HandleExceptionAsync(httpContext, exception);
        }

        public async Task HandleHttpRequestAsync(HttpContext httpConext)
        {
            var pipelineContext = new HttpContextPipelineHandlerContext(httpConext);
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
                    await _handlers[i].ProcessResponseAsync(pipelineContext);
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

            _handlers.Add(processor);
        }

        public void Insert(int index, IHttpContextPipelineHandler processor)
        {
            if (processor == null) throw new ArgumentNullException(nameof(processor));

            _handlers.Insert(index, processor);
        }

        public void InsertAfter<TBefore>(IHttpContextPipelineHandler processor) where TBefore : IHttpContextPipelineHandler
        {
            if (processor == null) throw new ArgumentNullException(nameof(processor));

            Insert(_handlers.FindIndex(h => h is TBefore) + 1, processor);
        }

        public void InsertBefore<TAfter>(IHttpContextPipelineHandler processor) where TAfter : IHttpContextPipelineHandler
        {
            Insert(_handlers.FindIndex(h => h is TAfter), processor);
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
