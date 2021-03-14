using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace AzureFunctionApp
{
    public class FaceAnalysisResults
    {
        public string ImageId { get; set; }
        public IList<DetectedFace> DetectedFaces { get; set; }
    }
}
