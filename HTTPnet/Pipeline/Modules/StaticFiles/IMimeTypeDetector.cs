namespace HTTPnet.Pipeline.Modules.StaticFiles
{
    public interface IMimeTypeDetector
    {
        string GetMimeTypeFromFilename(string filename);
    }
}
