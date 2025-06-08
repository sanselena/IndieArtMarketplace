using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IndieArtMarketplace.Business.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string containerName);
        Task DeleteFileAsync(string fileName, string containerName);
        string GetBlobUrl(string fileName, string containerName);
    }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _baseUrl;

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            _blobServiceClient = new BlobServiceClient(connectionString);
            _baseUrl = _blobServiceClient.Uri.ToString();
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, true);

            return fileName;
        }

        public async Task DeleteFileAsync(string fileName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }

        public string GetBlobUrl(string fileName, string containerName)
        {
            return $"{_baseUrl}/{containerName}/{fileName}";
        }
    }
} 