using System;
using System.IO;
using System.Threading.Tasks;

namespace HTTPnet.Pipeline.Modules.StaticFiles
{
    public class PhysicalStaticFilesStorage : IStaticFilesStorage
    {
        private readonly string _rootPath;

        public PhysicalStaticFilesStorage(string rootPath)
        {
            _rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
        }

        public bool Exists(string filename)
        {
            filename = Path.Combine(_rootPath, filename);
            return File.Exists(filename);
        }

        public Task<Stream> LoadAsync(string filename)
        {
            filename = Path.Combine(_rootPath, filename);
            return Task.FromResult((Stream)File.OpenRead(filename));
        }

        public async Task SaveAsync(string filename, Stream content)
        {
            filename = Path.Combine(_rootPath, filename);
            using (var fileStream = File.OpenWrite(filename))
            {
                await content.CopyToAsync(fileStream);
            }
        }

        public Task DeleteAsync(string filename)
        {
            filename = Path.Combine(_rootPath, filename);
            File.Delete(filename);
            return Task.FromResult(0);
        }
    }
}
