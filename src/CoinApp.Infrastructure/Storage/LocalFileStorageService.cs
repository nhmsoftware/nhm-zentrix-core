using CoinApp.Application.Common.Interfaces;
using CoinApp.Application.Common.Storage;
using Microsoft.Extensions.Configuration;

namespace CoinApp.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _rootPath = configuration["Storage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "storage", "uploads");
    }

    public async Task<StoredFile> SaveAsync(string folder, FileUpload file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        var safeFolder = folder.Replace('\\', '/').Trim('/');
        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var targetDirectory = Path.Combine(_rootPath, safeFolder);
        var targetPath = Path.Combine(targetDirectory, storedFileName);

        Directory.CreateDirectory(targetDirectory);

        await using var output = File.Create(targetPath);
        await file.Content.CopyToAsync(output, cancellationToken);

        var relativePath = Path.Combine(safeFolder, storedFileName).Replace('\\', '/');
        return new StoredFile(relativePath, storedFileName, file.ContentType, file.Length);
    }
}
