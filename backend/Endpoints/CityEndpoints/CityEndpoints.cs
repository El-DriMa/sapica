using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;
namespace sapica_backend.Endpoints.GeneralEndpoints
{
    [Route("cities")]
    [ApiController]
    public class CityEndpoints(ApplicationDbContext db) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetCities()
        {
            var cities = await db.City.ToListAsync();
            return Ok(cities);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCity(int id)
        {
            var city = await db.City.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }
            return Ok(city);
        }
    }
}