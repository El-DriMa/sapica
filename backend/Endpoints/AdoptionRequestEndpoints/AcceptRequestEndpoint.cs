using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;
using sapica_backend.Helper.Api;

namespace sapica_backend.Endpoints.AdoptionRequestEndpoints
{
    [Route("request")]
    public class AcceptRequestEndpoint(ApplicationDbContext db) : MyEndpointBaseAsync.WithRequest<int>.WithoutResult
    {
        [HttpPatch("{id}")]
        public  override async Task HandleAsync(int id, CancellationToken cancellationToken = default)
        {
            var req = await db.AdoptionRequest.Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
            if (req != null)
            {
                req.IsAccepted = true;
                var ap = await db.AdoptionPost.Where(x => x.Id == req.AdoptionPostId).FirstOrDefaultAsync(cancellationToken);
                ap.IsAdopted=true;
                ap.DateOfAdoption = DateTime.Now;
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
