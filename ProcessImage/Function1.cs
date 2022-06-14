using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs.Models;

namespace ProcessImage
{
    public class Function1
    {
        private readonly byte[][] _validFormats =
        {
            new byte[] { 0x42, 0x4D },                          // BMP "BM"
            new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 },     // "GIF87a"
            new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 },     // "GIF89a"
            new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },   // PNG "\x89PNG\x0D\0xA\0x1A\0x0A"
            new byte[] { 0x49, 0x49, 0x2A, 0x00 }, // TIFF II "II\x2A\x00"
            new byte[] { 0x4D, 0x4D, 0x00, 0x2A }, // TIFF MM "MM\x00\x2A"
            new byte[] { 0xFF, 0xD8, 0xFF },        // JPEG JFIF (SOI "\xFF\xD8" and half next marker xFF)
            new byte[] { 0xFF, 0xD9 }           // JPEG EOI "\xFF\xD9"
        };

        [FunctionName("Function1")]
        public async Task Run([BlobTrigger("{artistName}", Connection = "BlobConnectionString")]BlobClient blobClient, string artistName, string portraitName, ILogger log)
        {
            using var blob = await blobClient.OpenReadAsync();
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{artistName} \n Size: {blob.Length} Bytes");

            int largestFormatArray = _validFormats.Max(b => b.Length);

            BinaryReader reader = new BinaryReader(blob);
            var blobBytes = reader.ReadBytes(largestFormatArray);

            bool isImage = false;

            foreach (byte[] bytes in _validFormats)
            {
                bool isFormat = true;
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (bytes[i] != blobBytes[i])
                    {
                        isFormat = false;
                        break;
                    }
                }
                if (isFormat == true)
                {
                    isImage = true;
                    break;
                }
            }

            if (isImage == false)
            {
                await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
                log.LogInformation($"C# Blob trigger function Deleted blob {name}. Blob not a supported image.");
            }
        }
    }
}
