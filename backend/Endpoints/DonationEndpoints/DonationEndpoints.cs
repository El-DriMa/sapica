using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;
using sapica_backend.Data.Models;
using sapica_backend.Endpoints.UserEndpoints;
using sapica_backend.Services;

namespace sapica_backend.Endpoints.DonationEndpoints
{
    [Route("donations")]
    [ApiController]
    public class DonationEndpoints(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> AddDonation([FromBody] DonationCreateRequest request, CancellationToken cancellationToken)
        {
            var currentUser = await AuthService.GetCurrentUserAsync(db, httpContextAccessor, cancellationToken);

            if (currentUser == null)
            {
                return Unauthorized(new { message = "Unauthenticated." });
            }

            if (currentUser is not User userCheck)
            {
                return Forbid();
            }

            var CurrentUser = (User)currentUser;

            var shelter = await db.Shelter.Where(x => x.Id == request.ShelterId).FirstOrDefaultAsync(cancellationToken); 
            if (shelter == null)
            {
                return NotFound(new { message = "Shelter not found" });
            }

            if (request.Amount < 1)
            {
                return BadRequest(new { message = "Invalid amount" });
            }

            var donation = new Donation
            {
                DateTime = DateTime.Now,
                Amount = request.Amount,
                UserId = CurrentUser.Id,
                ShelterId = request.ShelterId
            };

            await db.Donation.AddAsync(donation, cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            return Ok(donation);
        }
    }


    public class DonationCreateRequest
    {
        public decimal Amount { get; set; }
        public int ShelterId { get; set; }
    }
}
