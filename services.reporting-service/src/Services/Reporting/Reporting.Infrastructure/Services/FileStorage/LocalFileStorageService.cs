using Microsoft.Extensions.Configuration;
using Reporting.Application.Contracts.Infrastructure;

namespace Reporting.Infrastructure.Services.FileStorage;

/// <summary>
/// A simple file storage service that saves files to a local disk path.
/// Note: This is not suitable for a scalable, distributed production environment.
/// In production, this would be replaced with a service for Azure Blob Storage, AWS S3, etc.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        // e.g., "FileStorage:BasePath": "C:/temp/sss-opc-reports"
        _basePath = configuration.GetValue<string>("FileStorage:BasePath") 
                    ?? throw new ArgumentNullException("FileStorage:BasePath is not configured.");

        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> SaveAsync(byte[] content, string fileName, string container, CancellationToken cancellationToken = default)
    {
        var containerPath = Path.Combine(_basePath, container);
        if (!Directory.Exists(containerPath))
        {
            Directory.CreateDirectory(containerPath);
        }

        var fullPath = Path.Combine(containerPath, fileName);

        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);

        // For a local service, the path is the identifier.
        // For a cloud service, this would return a URL.
        return fullPath;
    }
}