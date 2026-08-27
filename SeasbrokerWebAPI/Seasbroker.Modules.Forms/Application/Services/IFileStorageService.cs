using Microsoft.AspNetCore.Http;

namespace Seasbroker.Modules.Forms.Application.Services;

public interface IFileStorageService
{
    /// <summary>Saves the file under the given sub-folder and returns the path relative to the storage root.</summary>
    Task<string> SaveAsync(IFormFile file, string subFolder, CancellationToken cancellationToken = default);

    /// <summary>Opens a read stream for a previously stored file, given its relative path.</summary>
    Stream OpenRead(string relativePath);
}
