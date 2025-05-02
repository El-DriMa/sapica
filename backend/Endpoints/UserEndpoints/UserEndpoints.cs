using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;
using sapica_backend.Data.Models;
using sapica_backend.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading;

namespace sapica_backend.Endpoints.UserEndpoints
{
    [Route("users")]
    [ApiController]
    public class UserEndpoints(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, EmailService emailService) : ControllerBase
    {
        private string GenerateActivationToken()
        {
            return Guid.NewGuid().ToString();
        }

        private async Task SendActivationEmail(string email, string link)
        {
            await emailService.SendActivationEmail(email, link);
        }

        [Authorize]
        [HttpGet("")]
        public async Task<IActionResult> GetAllUsers(
            CancellationToken cancellationToken,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? firstName = null,
            [FromQuery] string? lastName = null,
            [FromQuery] int? minYearBorn = null,
            [FromQuery] int? maxYearBorn = null,
            [FromQuery] int? cityId = null,
            [FromQuery] string? sortOrder = "Ascending"
        )
        {
            var currentUser = await AuthService.GetCurrentUserAsync(db, httpContextAccessor, cancellationToken);

            if (currentUser == null)
            {
                return Unauthorized(new { message = "Unauthenticated." });
            }

            if (currentUser is not Admin adminCheck)
            {
                return Forbid();
            }

            var usersQuery = db.User.AsQueryable();

            if (!string.IsNullOrEmpty(firstName))
            {
                usersQuery = usersQuery.Where(x => x.FirstName.Contains(firstName));
            }

            if (!string.IsNullOrEmpty(lastName))
            {
                usersQuery = usersQuery.Where(x => x.LastName.Contains(lastName));
            }

            if (minYearBorn.HasValue)
            {
                usersQuery = usersQuery.Where(x => x.YearBorn >= minYearBorn.Value);
            }

            if (maxYearBorn.HasValue)
            {
                usersQuery = usersQuery.Where(x => x.YearBorn <= maxYearBorn.Value);
            }

            if (cityId.HasValue)
            {
                usersQuery = usersQuery.Where(x => x.CityId == cityId.Value);
            }

            var users = await usersQuery.Include(x => x.City).ToListAsync(cancellationToken);

            var userPostCounts = await db.AdoptionPost
                .GroupBy(p => p.Username)
                .Select(g => new
                {
                    Username = g.Key,
                    PostCount = g.Count()
                })
                .ToListAsync(cancellationToken);

            var usersWithPostCounts = users.Select(x => new
            {
                User = x,
                PostCount = userPostCounts.FirstOrDefault(y => y.Username == x.Username)?.PostCount ?? 0
            }).ToList();

            if (sortOrder == "Ascending")
            {
                usersWithPostCounts = usersWithPostCounts.OrderBy(x => x.PostCount).ToList();
            }
            else if (sortOrder == "Descending")
            {
                usersWithPostCounts = usersWithPostCounts.OrderByDescending(x => x.PostCount).ToList();
            }

            var totalUsers = usersWithPostCounts.Count;
            var pagedUsers = usersWithPostCounts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                TotalUsers = totalUsers,
                Page = page,
                PageSize = pageSize,
                Users = pagedUsers.Select(x => new UserReadResponse
                {
                    Id = x.User.Id,
                    FirstName = x.User.FirstName,
                    LastName = x.User.LastName,
                    YearBorn = x.User.YearBorn,
                    Username = x.User.Username,
                    Email = x.User.Email,
                    ImageUrl = x.User.ImageUrl,
                    PhoneNumber = x.User.PhoneNumber,
                    CityId = x.User.CityId,
                    City = x.User.City,
                    PostCount = x.PostCount
                })
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadResponse>> GetUserById(int id, CancellationToken cancellationToken)
        {
            var user = await db.User
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (user == null) return NotFound();

            var response = new UserReadResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                YearBorn = user.YearBorn,
                Username = user.Username,
                Email = user.Email,
                ImageUrl = user.ImageUrl,
                PhoneNumber = user.PhoneNumber,
                CityId = user.CityId
            };

            return Ok(response);
        }


        [HttpPost("")]
        public async Task<ActionResult> AddUser([FromBody] UserCreateRequest request, CancellationToken cancellationToken)
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

            var activationToken = GenerateActivationToken();
            var user = new User
            {
                isUser = true,
                FirstName = request.FirstName,
                LastName = request.LastName,
                YearBorn = request.YearBorn,
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password),
                Email = request.Email,
                ImageUrl = request.ImageUrl,
                PhoneNumber = request.PhoneNumber,
                CityId = request.CityId,
                City = db.City.Where(x => x.Id == request.CityId).First(),
                ActivationToken = activationToken,
                IsActivated = false,
            };

            var userAccount = new UserAccount
            {
                isUser = true,
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password),
                Email = request.Email,
                ImageUrl = request.ImageUrl,
                PhoneNumber = request.PhoneNumber,
                CityId = request.CityId,
                City = user.City
            };

            db.User.Add(user);
            db.UserAccount.Add(userAccount);
            await db.SaveChangesAsync(cancellationToken);
            var activationLink = $"https://localhost:7291/users/activate?token={activationToken}";
            await SendActivationEmail(user.Email, activationLink);

            var response = new UserReadResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                YearBorn = user.YearBorn,
                Username = user.Username,
                Email = user.Email,
                ImageUrl = user.ImageUrl,
                PhoneNumber = user.PhoneNumber,
                CityId = user.CityId,
                City = user.City
            };

            return CreatedAtAction(nameof(AddUser), new { id = user.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateRequest request, CancellationToken cancellationToken)
        {
            var currentUser = await AuthService.GetCurrentUserAsync(db, httpContextAccessor, cancellationToken);

            if (currentUser == null)
            {
                return Unauthorized(new { message = "Unauthenticated." });
            }

            if (currentUser is User userCheck)
            {
                if (userCheck.Id != id)
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }

            var CurrentUser = (User)currentUser;

            var existingUsername = await db.UserAccount
                .FirstOrDefaultAsync(x => x.Username == request.Username && x.Username != CurrentUser.Username, cancellationToken);

            if (existingUsername != null)
            {
                return BadRequest("Username is already taken.");
            }

            var existingEmail = await db.UserAccount
                .FirstOrDefaultAsync(x => x.Email == request.Email && x.Username != CurrentUser.Username, cancellationToken);

            if (existingEmail != null)
            {
                return BadRequest("Email is already in use.");
            }

            var cityExists = await db.City.AnyAsync(c => c.Id == request.CityId, cancellationToken);

            if (!cityExists)
            {
                return BadRequest(new { message = "Provided CityId does not exist in the database." });
            }

            var user = await db.User
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (user == null) return NotFound();

            var userAccount = await db.UserAccount
                .FirstOrDefaultAsync(x => x.Username == user.Username, cancellationToken);

            if (userAccount == null) return NotFound();

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.YearBorn = request.YearBorn;
            user.Username = request.Username;
            user.Email = request.Email;
            user.ImageUrl = request.ImageUrl;
            user.PhoneNumber = request.PhoneNumber;
            user.CityId = request.CityId;

            userAccount.Username = request.Username;
            userAccount.Email = request.Email;
            userAccount.ImageUrl = request.ImageUrl;
            userAccount.PhoneNumber = request.PhoneNumber;
            userAccount.CityId = request.CityId;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);
                userAccount.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);
            }

            await db.SaveChangesAsync(cancellationToken);

            var response = new UserReadResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                YearBorn = user.YearBorn,
                Username = user.Username,
                Email = user.Email,
                ImageUrl = user.ImageUrl,
                PhoneNumber = user.PhoneNumber,
                CityId = user.CityId
            };

            return Ok(response);
        }

        [HttpPatch("{userId}")]
        public async Task<IActionResult> PatchUser(int userId, [FromBody] UserPatchRequest userPatch)
        {
            var currentUser = await AuthService.GetCurrentUserAsync(db, httpContextAccessor);

            if (currentUser == null)
            {
                return Unauthorized(new { message = "Unauthenticated." });
            }

            if (currentUser is User userCheck)
            {
                if (userCheck.Id != userId)
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }

            var user = (User)currentUser;

            if (user == null) return NotFound();

            var userAccount = await db.UserAccount
                .FirstOrDefaultAsync(x => x.Username == user.Username);

            if (userAccount == null) return NotFound();

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            if (userPatch.FirstName != null)
            {
                user.FirstName = userPatch.FirstName;
            }

            if (userPatch.LastName != null)
            {
                user.LastName = userPatch.LastName;
            }

            if (userPatch.YearBorn.HasValue)
            {
                user.YearBorn = userPatch.YearBorn.Value;
            }

            if (userPatch.Username != null)
            {
                user.Username = userPatch.Username;
                userAccount.Username = userPatch.Username;
            }

            if (userPatch.Email != null)
            {
                user.Email = userPatch.Email;
                userAccount.Email = userPatch.Email;
            }

            if (userPatch.PhoneNumber != null)
            {
                user.PhoneNumber = userPatch.PhoneNumber;
                userAccount.PhoneNumber = userPatch.PhoneNumber;
            }

            if (userPatch.ImageUrl != null)
            {
                user.ImageUrl = userPatch.ImageUrl;
                userAccount.ImageUrl = userPatch.ImageUrl;
            }

            if (userPatch.CityId.HasValue)
            {
                user.CityId = userPatch.CityId.Value;
                userAccount.CityId = userPatch.CityId.Value;
            }

            db.User.Update(user);
            db.UserAccount.Update(userAccount);
            await db.SaveChangesAsync();

            var response = new UserReadResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                YearBorn = user.YearBorn,
                Username = user.Username,
                Email = user.Email,
                ImageUrl = user.ImageUrl,
                PhoneNumber = user.PhoneNumber,
                CityId = user.CityId
            };

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
        {
            var user = await db.User
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (user == null) return NotFound();

            var userAccount = await db.UserAccount
                .FirstOrDefaultAsync(x => x.Username == user.Username, cancellationToken);

            if (userAccount == null) return NotFound();

            db.User.Remove(user);
            db.UserAccount.Remove(userAccount);
            await db.SaveChangesAsync(cancellationToken);

            return Ok(new { message = "User deleted successfully." });
        }

        [HttpGet("check-username-availability")]
        public async Task<IActionResult> CheckUsernameAvailability([FromQuery] string username, CancellationToken cancellationToken)
        {
            bool usernameAvailable = false;

            if (!string.IsNullOrEmpty(username))
            {
                var usernameExists = await db.UserAccount.AnyAsync(x => x.Username == username, cancellationToken);
                usernameAvailable = !usernameExists;
            }

            return Ok(new
            {
                usernameAvailable
            });
        }

        [HttpGet("check-email-availability")]
        public async Task<IActionResult> CheckEmailAvailability([FromQuery] string email, CancellationToken cancellationToken)
        {
            bool emailAvailable = false;

            if (!string.IsNullOrEmpty(email))
            {
                var emailExists = await db.UserAccount.AnyAsync(x => x.Email == email, cancellationToken);
                emailAvailable = !emailExists;
            }

            return Ok(new
            {
                emailAvailable
            });
        }

        [HttpGet("activate")]
        public async Task<IActionResult> ActivateUser([FromQuery] string token, CancellationToken cancellationToken)
        {
            var user = await db.User.FirstOrDefaultAsync(u => u.ActivationToken == token, cancellationToken);

            if (user == null)
            {
                return BadRequest("Nevaljan aktivacijski token.");
            }

            if (user.IsActivated)
            {
                return BadRequest("Račun je već aktiviran.");
            }

            user.IsActivated = true;
            user.ActivationToken = string.Empty;
            await db.SaveChangesAsync(cancellationToken);

            return Ok("Vaš račun je uspješno aktiviran.");
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
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

            var user = (User)currentUser;

            var userAccount = await db.UserAccount.FirstOrDefaultAsync(u => u.Username == user.Username, cancellationToken);
            if (userAccount == null)
                return NotFound("User not found.");

            var passwordVerificationResult = BCrypt.Net.BCrypt.EnhancedVerify(request.CurrentPassword, userAccount.Password);
            if (!passwordVerificationResult)
                return BadRequest(new { detail = "Current password is incorrect." });

            user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.NewPassword);
            userAccount.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.NewPassword);

            await db.SaveChangesAsync(cancellationToken);

            return Ok(new { message = "Password changed successfully." });
        }
    }

    public class UserCreateRequest
    {
        [MinLength(2, ErrorMessage = "FirstName must be over 2 letters."), 
            MaxLength(20, ErrorMessage = "FirstName must be under 20 letters.")]
        public required string FirstName { get; set; }

        [MinLength(2, ErrorMessage = "LastName must be over 2 letters."), 
            MaxLength(30, ErrorMessage = "LastName must be under 30 letters.")]
        public required string LastName { get; set; }

        [Range(1930, 2010, ErrorMessage = "Age must be at least 15.")]
        public required int YearBorn { get; set; }

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

    public class UserReadResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int YearBorn { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; }
        public string PhoneNumber { get; set; }
        public int CityId { get; set; }
        public City City {  get; set; }
        public int? PostCount {  get; set; }
    }

    public class UserUpdateRequest
    {
        [MaxLength(20, ErrorMessage = "FirstName must be under 20 letters.")]
        public required string FirstName { get; set; }

        [MaxLength(30, ErrorMessage = "LastName must be under 30 letters.")]
        public required string LastName { get; set; }

        [Range(1900, 2010, ErrorMessage = "Age must be at least 15.")]
        public required int YearBorn { get; set; }

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

    public class UserPatchRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? YearBorn { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ImageUrl { get; set; }
        public int? CityId { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = null!;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d\W_]{8,}$", ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one number.")]
        public string NewPassword { get; set; } = null!;
    }
}
