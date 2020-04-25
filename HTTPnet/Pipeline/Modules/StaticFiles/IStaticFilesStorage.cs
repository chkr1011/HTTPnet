using System.IO;
using System.Threading.Tasks;

namespace HTTPnet.Pipeline.Modules.StaticFiles
{
    public interface IStaticFilesStorage
    {
        bool Exists(string filename);

        Task<Stream> LoadAsync(string filename);

        Task SaveAsync(string filename, Stream content);

        Task DeleteAsync(string filename);
    }
}
