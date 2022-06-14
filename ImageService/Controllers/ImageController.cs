using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ImageService.Data.RequestData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ImageService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly BlobServiceClient blobClient;
        private readonly ComputerVisionClient visionClient;

        public ImageController(BlobServiceClient blobClient, ComputerVisionClient visionClient)
        {
            this.blobClient = blobClient;
            this.visionClient = visionClient;
        }

        // GET: api/<ImageController>
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            List<string> list = new List<string>();
            await foreach (var item in  blobClient.GetBlobContainersAsync())
            {
                list.Add(item.Name);
            }
            return list;
        }

        // GET api/<ImageController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ImageController>
        [HttpPost]
        public async Task<IActionResult> PostAsync([ModelBinder(BinderType = typeof(JsonModelBinder))] ImagePostRequest requestData, IFormFile file)
        {
            if (requestData == null)
                return BadRequest("Request data is required");

            if (requestData.ArtistName == null || requestData.ArtistName == string.Empty)
                return BadRequest("Request data is invalid. Artist name must be provided");
            else if (requestData.ImageName == null || requestData.ImageName == string.Empty)
                return BadRequest("Request data is invalid. Image name must be provided");

            var containerClient = blobClient.GetBlobContainerClient(blobContainerName: requestData.ArtistName.ToLower());
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
            var client = containerClient.GetBlobClient(requestData.ImageName.ToLower());


            // Specify the features to return  
            List<VisualFeatureTypes?> features =
            new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description, VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult
            };

            using (var stream = file.OpenReadStream())
            {
                var analysisResult = await visionClient.AnalyzeImageInStreamAsync(stream, features);
                if (analysisResult.Adult.IsAdultContent && analysisResult.Adult.AdultScore > 0.6)
                    return BadRequest("No Adult content allowed");
            }
            using (var stream = file.OpenReadStream())
            {
                var uploadResult = await client.UploadAsync(stream);
                return Ok(uploadResult);
            }
        }

        // DELETE api/<ImageController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
