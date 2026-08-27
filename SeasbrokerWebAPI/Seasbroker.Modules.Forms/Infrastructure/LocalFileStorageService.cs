using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Seasbroker.Modules.Forms.Application.Services;

namespace Seasbroker.Modules.Forms.Infrastructure;

public class FormsStorageOptions
{
    /// <summary>Root folder for uploaded form-submission files. Never served as static content.</summary>
    public string RootPath { get; set; } = "App_Data/form-uploads";
}

/// <summary>
/// Stores form-submission file uploads on local disk. Files are never exposed via static file
/// hosting - they're only readable through the authenticated download endpoint.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _root;

    public LocalFileStorageService(IOptions<FormsStorageOptions> options)
    {
        _root = Path.GetFullPath(options.Value.RootPath);
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(IFormFile file, string subFolder, CancellationToken cancellationToken = default)
    {
        var safeFolder = Path.Combine(subFolder.Split('/', '\\').Select(SanitizeSegment).ToArray());
        var directory = Path.Combine(_root, safeFolder);
        Directory.CreateDirectory(directory);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(directory, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return Path.Combine(safeFolder, fileName).Replace('\\', '/');
    }

    public Stream OpenRead(string relativePath)
    {
        var fullPath = ResolveSafePath(relativePath);
        return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
    }

    private string ResolveSafePath(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_root, relativePath));
        if (!fullPath.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Invalid storage path.");
        }

        return fullPath;
    }

    private static string SanitizeSegment(string segment)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(segment.Where(c => !invalid.Contains(c)).ToArray());
    }
}
