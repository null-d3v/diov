using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Diov.Web;

public class NoWatchFileProvider(
    IFileProvider fileProvider) :
    IFileProvider
{
    public IFileProvider FileProvider { get; } =
        fileProvider;

    public IDirectoryContents GetDirectoryContents(
        string subpath)
    {
        return FileProvider
            .GetDirectoryContents(subpath);
    }

    public IFileInfo GetFileInfo(
        string subpath)
    {
        return FileProvider
            .GetFileInfo(subpath);
    }

    public IChangeToken Watch(
        string filter)
    {
        return NullChangeToken.Singleton;
    }
}
