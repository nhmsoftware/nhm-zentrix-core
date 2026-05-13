using CoinApp.Application.Common.Storage;

namespace CoinApp.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<StoredFile> SaveAsync(string folder, FileUpload file, CancellationToken cancellationToken = default);
}
