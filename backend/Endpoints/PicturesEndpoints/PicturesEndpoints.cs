using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using sapica_backend.Data;
using Microsoft.AspNetCore.Http;
using sapica_backend.Data.Models;
using sapica_backend.Services;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace sapica_backend.Endpoints.PicturesEndpoints
{
    [Route("api/[controller]")]
    [ApiController]
    public class PicturesEndpoints (ApplicationDbContext db, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly string azureConnectionString = "string";
        private readonly string containerName = "sapica";

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Nema datoteke za otpremanje." });
            }

            try
            {
                
                var blobServiceClient = new BlobServiceClient(azureConnectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                
                var blobName = $"{DateTime.UtcNow.Ticks}-{file.FileName}";
                var blobClient = containerClient.GetBlobClient(blobName);

                
                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                
                var imageUrl = blobClient.Uri.ToString();
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška prilikom otpremanja slike: {ex.Message}");
                return StatusCode(500, new { message = "Greška na serveru." });
            }
        }

        [HttpPost("update-user-image")]
        public async Task<IActionResult> UpdateUserImage(IFormFile file)
        {
            var currentUser = await AuthService.GetCurrentUserAsync(db, httpContextAccessor);

            if (currentUser == null)
            {
                return Unauthorized(new { message = "Unauthenticated." });
            }

            if (currentUser is not User userCheck)
            {
                return Forbid();
            }

            var user = (User)currentUser;

            var userAccount = await db.UserAccount.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (userAccount == null)
                return NotFound("User not found.");

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Nema datoteke za otpremanje." });
            }

            try
            {
                var blobServiceClient = new BlobServiceClient(azureConnectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);


                var blobName = $"{DateTime.UtcNow.Ticks}-{file.FileName}";
                var blobClient = containerClient.GetBlobClient(blobName);


                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                var imageUrl = blobClient.Uri.ToString();
                user.ImageUrl = imageUrl;
                userAccount.ImageUrl = imageUrl;
                db.SaveChanges();
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška prilikom otpremanja slike: {ex.Message}");
                return StatusCode(500, new { message = "Greška na serveru." });
            }
        }
    }
}

