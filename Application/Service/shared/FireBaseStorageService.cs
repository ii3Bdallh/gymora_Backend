using System.Net;
using Application.Interface.Service.Shared;
using Application.Interface.Service.Shared.Application.Interface.Service.Shared;
using Domain.Options;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;

namespace Application.Service.Shared
{
    public class FirebaseStorageService(
        StorageClient storageClient,
        GoogleCredential googleCredential,
        FirebaseStorageOptions firebaseStorageOptions)
        : IStorageService
    {
        public async Task<string> UploadFileToStorageAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be null or empty");

            string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";

            using (var stream = file.OpenReadStream())
            {
                var metadata = new Google.Apis.Storage.v1.Data.Object
                {
                    Bucket = firebaseStorageOptions.BucketName,
                    Name = uniqueFileName,
                    ContentType = file.ContentType
                };

                await storageClient.UploadObjectAsync(metadata, stream, cancellationToken: cancellationToken);
            }

            return GenerateUrlToAccessFileAsync(uniqueFileName);
        }

        public string GenerateUrlToAccessFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            // لازم الـ Credential يكون ServiceAccountCredential عشان UrlSigner يقدر يعمل توقيع فعلي
            if (googleCredential.UnderlyingCredential is not ServiceAccountCredential serviceAccountCredential)
            {
                throw new InvalidOperationException(
                    "Signed URLs require a Service Account credential (JSON key file), not Application Default Credentials.");
            }

            UrlSigner urlSigner = UrlSigner.FromCredential(serviceAccountCredential);

            return urlSigner.Sign(
                firebaseStorageOptions.BucketName,
                fileName,
                TimeSpan.FromDays(365 * 100), // 100 سنة
                HttpMethod.Get
            );
        }

        public async Task<bool> DeleteFileFromStorageAsync(string fileName, CancellationToken cancellationToken = default)
        {
            try
            {
                await storageClient.DeleteObjectAsync(firebaseStorageOptions.BucketName, fileName, cancellationToken: cancellationToken);
                return true;
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCollectionFromStorageAsync(string collectionPath, CancellationToken cancellationToken = default)
        {
            try
            {
                string prefix = collectionPath.Trim('/') + "/";
                var objects = storageClient.ListObjectsAsync(firebaseStorageOptions.BucketName, prefix);
                bool anyDeleted = false;

                await foreach (var obj in objects.WithCancellation(cancellationToken))
                {
                    await storageClient.DeleteObjectAsync(firebaseStorageOptions.BucketName, obj.Name, cancellationToken: cancellationToken);
                    anyDeleted = true;
                }

                return anyDeleted;
            }
            catch
            {
                return false;
            }
        }
    }
}