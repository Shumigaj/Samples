using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureFunctionApp
{
    public static class ImageUploaded
    {
        [FunctionName("ImageUploaded")]
        public static void Run(
            [BlobTrigger("container-name/{name}", Connection = "AccountStorageConnection")]CloudBlockBlob blob, 
            string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{blob.Name} \n Size: {blob.Properties.Length} Bytes");

            var blobUrl = $"{blob.Uri}{GetSas(blob)}";
            log.LogInformation($"Blob URL: {blobUrl}");
        }

        public static string GetSas(CloudBlockBlob blob)
        {
            var sasPolicy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-15), // adjust for clocks difference
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15)
            };

            var sharedAccessSignature = blob.GetSharedAccessSignature(sasPolicy);
            return sharedAccessSignature;
        }
    }
}
