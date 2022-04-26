using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Azure.Storage.Blobs.Models;
using ImageService.Data.RequestData;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ImageService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly BlobServiceClient blobClient;

        public ImageController(BlobServiceClient blobClient)
        {
            this.blobClient = blobClient ?? throw new ArgumentNullException(nameof(blobClient));
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

            using var stream = file.OpenReadStream();
            await client.UploadAsync(stream);
            
            return Ok();
        }

        // DELETE api/<ImageController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
