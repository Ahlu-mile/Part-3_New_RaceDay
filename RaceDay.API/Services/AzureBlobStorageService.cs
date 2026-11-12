using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services
{
    public class AzureBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobStorageService(
          IConfiguration configuration)
        {
            var connectionString =
              configuration["AzureStorage:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                  "Azure Blob Storage connection string is missing.");
            }

            _blobServiceClient =
              new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadAsync(
          IFormFile file,
          string containerName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file was provided.");

            var container =
              _blobServiceClient.GetBlobContainerClient(containerName);

            await container.CreateIfNotExistsAsync(
              PublicAccessType.None);

            var extension = Path.GetExtension(file.FileName);
            var blobName = $"{Guid.NewGuid()}{extension}";

            var blob = container.GetBlobClient(blobName);

            await using var stream = file.OpenReadStream();

            await blob.UploadAsync(
              stream,
              new BlobUploadOptions
              {
                  HttpHeaders = new BlobHttpHeaders
                  {
                      ContentType = file.ContentType
                  }
              });

            return blobName;
        }
    }
}
