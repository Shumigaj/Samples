using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureFunctionApp
{
    public static class ImageUploaded
    {
        [FunctionName("ImageUploaded")]
        public static async void Run(
            [BlobTrigger("container-name/{name}", Connection = "AccountStorageConnection")] CloudBlockBlob blob,
            string name, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{blob.Name} \n Size: {blob.Properties.Length} Bytes");

            var blobUrl = $"{blob.Uri}{GetSas(blob)}";
            log.LogInformation($"Blob URL: {blobUrl}");

            try
            {
                var facesInformation = await GetAnalisysAsync(blobUrl, context).ConfigureAwait(false);

                log.LogInformation("Image: '{imageName}' has {PeopleOnImage}", blob.Name, facesInformation.Length);

                for (int i = 0; i < facesInformation.Length; i++)
                {
                    log.LogInformation("Image: '{imageName}' person #{PersonOnImage}: {Gender}, {Age}, {Glasses}, {Emotion}",
                        blob.Name,
                        facesInformation[i].FaceAttributes.Gender,
                        facesInformation[i].FaceAttributes.Age,
                        facesInformation[i].FaceAttributes.Glasses,
                        facesInformation[i].FaceAttributes.Emotion);
                }
            }
            catch (Exception e)
            {
                log.LogError("Failed to process faces. {Error}", e.Message);
            }
        }

        public static Task<Face[]> GetAnalisysAsync(string imageUrl, ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var client = new FaceServiceClient(config["FaceCognitiveServiceKey"], config["FaceCognitiveServiceRoot"]);

            var attributes = new[] { FaceAttributeType.Gender };

            return client.DetectAsync(imageUrl, false, false, attributes);
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
