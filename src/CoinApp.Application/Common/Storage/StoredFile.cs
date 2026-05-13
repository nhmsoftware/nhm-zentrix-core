namespace CoinApp.Application.Common.Storage;

public sealed record StoredFile(string Path, string FileName, string ContentType, long Length);
