using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;
using sapica_backend.Helper.Api;

namespace sapica_backend.Endpoints.StatisticEndpoints
{
    [Route("adoption-statistics")]
    public class AdoptionStatisticEndpoint(ApplicationDbContext context):ControllerBase
    {
        [HttpGet]
        public IActionResult GetAdoptionStatistics()
        {
            var statistics = context.AdoptionPost
                .Where(post => post.IsAdopted)
                .GroupBy(post => post.DateOfAdoption.Date)
                .Select(group => new
                {
                    Date = group.Key,
                    Count = group.Count()
                })
                .OrderBy(stat => stat.Date)
                .ToList();

            return Ok(statistics);
        }
    }
}
