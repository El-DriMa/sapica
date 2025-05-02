using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;
using sapica_backend.Data.Models;
using sapica_backend.Endpoints.UserEndpoints;
using System.ComponentModel.DataAnnotations;

namespace sapica_backend.Endpoints.AdminEndpoints
{
    [Route("/admin")]
    [ApiController]
    public class AdminEndpoints(ApplicationDbContext db) : ControllerBase
    {
        [HttpPost("")]
        public async Task<IActionResult> AddAdmin([FromBody] AdminCreateRequest request, CancellationToken cancellationToken)
        {
            var existingUsername = await db.UserAccount
                .FirstOrDefaultAsync(x => x.Username == request.Username, cancellationToken);

            if (existingUsername != null)
            {
                return BadRequest("Username is already taken.");
            }

            var existingEmail = await db.UserAccount
                .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

            if (existingEmail != null)
            {
                return BadRequest("Email is already in use.");
            }

            var cityExists = await db.City.AnyAsync(c => c.Id == request.CityId, cancellationToken);

            if (!cityExists)
            {
                return BadRequest(new { message = "Provided CityId does not exist in the database." });
            }

            var admin = new Admin
            {
                isAdmin = true,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password),
                Email = request.Email,
                ImageUrl = request.ImageUrl,
                PhoneNumber = request.PhoneNumber,
                CityId = request.CityId,
                City = db.City.Where(x => x.Id == request.CityId).First()
            };

            var userAccount = new UserAccount
            {
                isAdmin = true,
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password),
                Email = request.Email,
                ImageUrl = request.ImageUrl,
                PhoneNumber = request.PhoneNumber,
                CityId = request.CityId,
                City = admin.City
            };

            db.Admin.Add(admin);
            db.UserAccount.Add(userAccount);
            await db.SaveChangesAsync(cancellationToken);

            var response = new AdminReadResponse
            {
                Id = admin.Id,
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                Username = admin.Username,
                Email = admin.Email,
                ImageUrl = admin.ImageUrl,
                PhoneNumber = admin.PhoneNumber,
                CityId = admin.CityId,
                City = admin.City
            };

            return CreatedAtAction(nameof(AddAdmin), new { id = admin.Id }, response);
        }
    }

    public class AdminCreateRequest
    {
        [MinLength(2, ErrorMessage = "FirstName must be over 2 letters."),
            MaxLength(20, ErrorMessage = "FirstName must be under 20 letters.")]
        public required string FirstName { get; set; }

        [MinLength(2, ErrorMessage = "LastName must be over 2 letters."),
            MaxLength(30, ErrorMessage = "LastName must be under 30 letters.")]
        public required string LastName { get; set; }

        [MinLength(1, ErrorMessage = "Username must have at least 1 character."),
            MaxLength(20, ErrorMessage = "Username cannot be longer than 20 characters."),
            RegularExpression("^[a-zA-Z0-9_]*$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
        public required string Username { get; set; }

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d\W_]{8,}$", ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one number.")]
        public required string Password { get; set; }

        [EmailAddress(ErrorMessage = "Email has to be in a valid format.")]
        public required string Email { get; set; }

        [RegularExpression(@"^(\+387|0)[6][0-7][0-9][0-9][0-9][0-9][0-9][0-9]$", ErrorMessage = "PhoneNumber must be a valid phone number for Bosnia and Herzegovina.")]
        public required string PhoneNumber { get; set; }

        public string ImageUrl { get; set; }

        public required int CityId { get; set; }
    }

    public class AdminReadResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; }
        public string PhoneNumber { get; set; }
        public int CityId { get; set; }
        public City City { get; set; }
    }
}
