using System;
using System.Net;
using System.Threading.Tasks;
using HTTPnet.Http;

namespace HTTPnet.Pipeline.Modules.StaticFiles
{
    public class StaticFilesModule : IHttpRequestPipelineModule
    {
        private readonly string _relativeUri;
        private readonly IStaticFilesStorage _storage;
        private readonly IMimeTypeDetector _mimeTypeDetector;

        public StaticFilesModule(string relativeUri, IStaticFilesStorage storage, IMimeTypeDetector mimeTypeDetector)
        {
            _relativeUri = relativeUri ?? throw new ArgumentNullException(nameof(relativeUri));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _mimeTypeDetector = mimeTypeDetector ?? throw new ArgumentNullException(nameof(mimeTypeDetector));

            if (!relativeUri.StartsWith("/"))
            {
                _relativeUri = "/" + _relativeUri;
            }

            if (!relativeUri.EndsWith("/"))
            {
                _relativeUri += "/";
            }
        }

        public bool AllowGet { get; set; } = true;
        public bool AllowSave { get; set; } = false;
        public bool AllowDelete { get; set; } = false;

        public async Task ProcessRequestAsync(HttpRequestPipelineModuleContext context)
        {
            if (!context.HttpContext.Request.Uri.LocalPath.StartsWith(_relativeUri, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var filename = context.HttpContext.Request.Uri.LocalPath;
            filename = filename.Substring(_relativeUri.Length);
            filename = filename.TrimStart('/');

            if (context.HttpContext.Request.Method == HttpMethod.Get && AllowGet)
            {
                if (!_storage.Exists(filename))
                {
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                context.HttpContext.Response.Body = await _storage.LoadAsync(filename).ConfigureAwait(false);

                var mimeType = _mimeTypeDetector.GetMimeTypeFromFilename(filename);
                if (!string.IsNullOrEmpty(mimeType))
                {
                    context.HttpContext.Response.Headers[HttpHeader.ContentType] = mimeType;
                }

                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            else if (context.HttpContext.Request.Method == HttpMethod.Post && AllowSave)
            {
                await _storage.SaveAsync(filename, context.HttpContext.Response.Body).ConfigureAwait(false);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            else if (context.HttpContext.Request.Method == HttpMethod.Delete && AllowDelete)
            {
                await _storage.DeleteAsync(filename).ConfigureAwait(false);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            }
        }

        public Task ProcessResponseAsync(HttpRequestPipelineModuleContext context)
        {
            return Task.FromResult(0);
        }
    }
}
