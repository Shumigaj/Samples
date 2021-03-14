using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {blob.Properties.Length} Bytes");

            var blobUrl = $"{blob.Uri}{GetSas(blob)}";
            log.LogInformation($"Blob URL: {blobUrl}");

            try
            {
                var facesInformation = await GetAnalisysAsync(blobUrl, context).ConfigureAwait(false);

                log.LogInformation("Image: '{imageName}' has {PeopleOnImage}", name, facesInformation.Count);

                for (var i = 0; i < facesInformation.Count; i++)
                {
                    log.LogInformation("Image: '{imageName}' person #{PersonOnImage}: Gender: {Gender}, Age: {Age}, Glasses: {Glasses}, Emotion: {Emotion}",
                        name,
                        i,
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

        public static Task<IList<DetectedFace>> GetAnalisysAsync(string imageUrl, ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var client = new FaceClient(new ApiKeyServiceClientCredentials(config["FaceCognitiveServiceKey"])) 
            { 
                Endpoint = config["FaceCognitiveServiceRoot"] 
            };

            var attributes = new List<FaceAttributeType?> { FaceAttributeType.Age };

            return client.Face.DetectWithUrlAsync(imageUrl, false, false, returnFaceAttributes: attributes);
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
