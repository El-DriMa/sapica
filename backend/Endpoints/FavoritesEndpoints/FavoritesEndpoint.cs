using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;
using sapica_backend.Data.Models;
using sapica_backend.Services;

namespace sapica_backend.Endpoints.FavoritesEndpoints
{
    [Route("favorites")]
    [ApiController]
    public class FavoritesEndpoint(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        [HttpGet("byUsername")]
        public async Task<ActionResult> GetFavoritesByUserId(CancellationToken cancellationToken = default)
        {
            var username = await GetUsernameFromCurrentUserAsync(cancellationToken);

            if (username == null)
            {
                return Unauthorized(new { message = "Unauthenticated." });
            }

            var favorites = await db.Favourite.Include(x => x.User).Include(x => x.AdoptionPost).ThenInclude(x => x.Animal).ThenInclude(x => x.Images).Where(x => x.User.Username == username).
                Select(x => new FavoritesReadResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    AdoptionPostId = x.AdoptionPostId,
                    AdoptionPost = x.AdoptionPost

                }).ToListAsync(cancellationToken);

            if (!favorites.Any()) { return Ok(new List<FavoritesReadResponse>()); }

            return Ok(favorites);

        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateFavorite(int adoptionPostId, CancellationToken cancellationToken = default)
        {
            var username = await GetUsernameFromCurrentUserAsync(cancellationToken);
            var user = await db.User.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
            if (user == null) return Unauthorized();

            int userId = user.Id;

            bool isOwnPost = await db.AdoptionPost.AnyAsync(x => x.Id == adoptionPostId && x.Username == username, cancellationToken);
            if (isOwnPost) return BadRequest("You cannot favorite your own post.");

           
            bool alreadyFavorited = await db.Favourite.AnyAsync(x => x.AdoptionPostId == adoptionPostId && x.UserId == userId, cancellationToken);
            if (alreadyFavorited) return BadRequest("This post is already in your favorites.");

            Favourite favorite = new()
            {
                AdoptionPostId = adoptionPostId,
                UserId = userId
            };

            db.Favourite.Add(favorite);
            await db.SaveChangesAsync(cancellationToken);

            return Ok(favorite);
        }

        [HttpDelete("delete")]
        public async Task<ActionResult> DeleteFavorite(int favoriteId,CancellationToken cancellationToken = default)
        {
            var username = await GetUsernameFromCurrentUserAsync(cancellationToken);
            if (username == null)
            {
                return Unauthorized(new { message = "Unauthenticated." });
            }

            var user = await db.User.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
            if (user == null) return Unauthorized();

            int userId = user.Id;


            var favorite = await db.Favourite.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == favoriteId, cancellationToken);

            if (favorite == null) return NotFound();

            if (favorite.UserId != userId)
            {
                return Unauthorized(new { message = "You are not authorized to delete this post." });
            }

            db.Favourite.Remove(favorite);

            await db.SaveChangesAsync(cancellationToken);

            return Ok(new { message = "Favorite deleted successfully" });

        }



        [HttpGet("username")]
        public async Task<string?> GetUsernameFromCurrentUserAsync(CancellationToken cancellationToken)
        {
            var currentUser = await AuthService.GetCurrentUserAsync(db, httpContextAccessor, cancellationToken);

            if (currentUser == null)
            {
                return null;
            }


            return currentUser switch
            {
                User userCheck => userCheck.Username,
                Shelter shelterCheck => shelterCheck.Username,
                _ => null
            };
        }
    }

    public class FavoritesReadResponse
    {
        public int Id { get; set; }
        public User? User { get; set; }
        public int UserId { get; set; }
        public AdoptionPost? AdoptionPost { get; set; }
        public int AdoptionPostId { get; set; }

    }
}
