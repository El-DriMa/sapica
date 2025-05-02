using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;

namespace sapica_backend.Endpoints.ShelterEndpoints
{
    public class GetShelterImages(ApplicationDbContext db):ControllerBase
    {
        [HttpGet("shelters/images")]
        public async Task<IActionResult> GetShelterImagess()
        {

           
            var imageLinks = await db.Shelter
                .Where(s => !string.IsNullOrEmpty(s.ImageUrl)) 
                .Select(s => s.ImageUrl)
                .ToListAsync();

            return Ok(imageLinks);
        }

    }
}
