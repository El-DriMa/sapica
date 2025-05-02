using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;
using sapica_backend.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using sapica_backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;


namespace sapica_backend.Endpoints.ShelterEndpoints
{
    
    [Route("shelters")]
    [ApiController]
    public class ShelterEndpoints : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext db;
        private readonly IHttpContextAccessor httpContextAccessor;
        public ShelterEndpoints(ApplicationDbContext _db,EmailService emailService, IHttpContextAccessor HttpContextAccessor)
        {
            
          _emailService = emailService;
            db = _db;
            httpContextAccessor = HttpContextAccessor;
        }
        private string GenerateActivationToken()
        {
            return Guid.NewGuid().ToString();
        }
        private async Task SendActivationEmail(string email, string link)
        {
            await _emailService.SendActivationEmail(email, link);
        }

        [Authorize]
        [HttpGet("cmb")]
        public async Task<IActionResult> GetShelters()
        {
            var shelters = await db.Shelter.ToListAsync();
            return Ok(shelters);
        }

        [Authorize]
        [HttpGet("")]
        public async Task<IActionResult> GetAllShelters(
            CancellationToken cancellationToken,
            [FromQuery] int? cityId,
            [FromQuery] string? name,
            [FromQuery] string? owner,
            [FromQuery] int? yearFounded,
            [FromQuery] string? username,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
            )
        {

            var currentUser = await AuthService.GetCurrentUserAsync(db, httpContextAccessor, cancellationToken);

           if (currentUser == null)
            {
                return Unauthorized(new { message = "Unauthenticated." });
            }

            if (currentUser is not Admin userCheck)
            {
                return Forbid();
            }
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and pageSize must be greater than 0.");

            
            var query = db.Shelter.AsQueryable();

           
            if (cityId.HasValue)
                query = query.Where(s => s.CityId == cityId);

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(s => s.Name.Contains(name));

            if (!string.IsNullOrWhiteSpace(owner))
                query = query.Where(s => s.Owner.Contains(owner));

            if (yearFounded.HasValue)
                query = query.Where(s => s.YearFounded == yearFounded);

            if (!string.IsNullOrWhiteSpace(username))
                query = query.Where(s => s.Username.Contains(username));

           
            var totalItems = await query.CountAsync(cancellationToken);


            /* var response = await db.Shelter
                 .Select(s => new ShelterReadResponse
                 {
                     Id = s.Id,
                     Name = s.Name,
                     Owner = s.Owner,
                     YearFounded = s.YearFounded,
                     Address = s.Address,
                     Username = s.Username,
                     Email = s.Email,
                     ImageUrl = s.ImageUrl,
                     PhoneNumber = s.PhoneNumber,
                     CityId = s.CityId
                 })
                 .ToArrayAsync(cancellationToken);*/
            var shelters = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ShelterReadResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Owner = s.Owner,
                    YearFounded = s.YearFounded,
                    Address = s.Address,
                    Username = s.Username,
                    Email = s.Email,
                    ImageUrl = s.ImageUrl,
                    PhoneNumber = s.PhoneNumber,
                    CityId = s.CityId
                })
                .ToListAsync(cancellationToken);

            var response = new
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                Shelters = shelters
            };

            return Ok(response);
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<ShelterReadResponse>> GetShelterById(int id, CancellationToken cancellationToken)
        {
            var currentUser = await AuthService.GetCurrentUserAsync(db, httpContextAccessor, cancellationToken);

            if (currentUser == null)
            {
                return Unauthorized(new { message = "Unauthenticated." });
            }

            if (currentUser is Shelter userCheck)
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

            var shelter = await db.Shelter
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (shelter == null) return NotFound();

            var response = new ShelterReadResponse
            {
                Id = shelter.Id,
                Name = shelter.Name,
                Owner = shelter.Owner,
                YearFounded = shelter.YearFounded,
                Address = shelter.Address,
                Username = shelter.Username,
                Email = shelter.Email,
                ImageUrl = shelter.ImageUrl,
                PhoneNumber = shelter.PhoneNumber,
                CityId = shelter.CityId,
                
            };

            return Ok(response);
        }



        [HttpPost("")]
        public async Task<IActionResult> AddShelter([FromBody] ShelterCreateRequest request, CancellationToken cancellationToken)
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
            var shelter = new Shelter
            {
                Name = request.Name,
                Owner = request.Owner,
                YearFounded = request.YearFounded,
                Address = request.Address,
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password),
                Email = request.Email,
                ImageUrl = request.ImageUrl,
                PhoneNumber = request.PhoneNumber,
                CityId = request.CityId,
                isShelter = true,
                ActivationToken=activationToken,
                IsActivated = false,
            };
            var userAccount = new UserAccount
            {
                isShelter = true,
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password),
                Email = request.Email,
                ImageUrl = request.ImageUrl,
                PhoneNumber = request.PhoneNumber,
                CityId = request.CityId,
                
            };

            db.Shelter.Add(shelter);
            db.UserAccount.Add(userAccount);
            await db.SaveChangesAsync(cancellationToken);
            var activationLink = $"https://localhost:7291/shelters/activate?token={activationToken}";
            await SendActivationEmail(shelter.Email, activationLink);
            return CreatedAtAction(nameof(AddShelter), new { id = shelter.Id }, shelter);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShelter(int id, [FromBody] ShelterUpdateRequest request, CancellationToken cancellationToken)
        {
            var currentUser = await AuthService.GetCurrentUserAsync(db, httpContextAccessor, cancellationToken);

            if (currentUser == null)
            {
                return Unauthorized(new { message = "Unauthenticated." });
            }

            if (currentUser is Shelter userCheck)
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

            var shelter = (Shelter)currentUser;

            var existingUsername = await db.UserAccount
                .FirstOrDefaultAsync(x => x.Username == request.Username && x.Username != shelter.Username, cancellationToken);

            if (existingUsername != null)
            {
                return BadRequest("Username is already taken.");
            }

            var existingEmail = await db.UserAccount
                .FirstOrDefaultAsync(x => x.Email == request.Email && x.Username != shelter.Username, cancellationToken);

            if (existingEmail != null)
            {
                return BadRequest("Email is already in use.");
            }

            var cityExists = await db.City.AnyAsync(c => c.Id == request.CityId, cancellationToken);

            if (!cityExists)
            {
                return BadRequest(new { message = "Provided CityId does not exist in the database." });
            }
            var shelterr = shelter;

            if (shelterr == null) return NotFound();
            var userAccount = await db.UserAccount
               .FirstOrDefaultAsync(x => x.Username == shelter.Username, cancellationToken);

            if (userAccount == null) return NotFound();

            shelter.Name = request.Name;
            shelter.Owner = request.Owner;
            shelter.YearFounded = request.YearFounded;
            shelter.Address = request.Address;
            shelter.Username = request.Username;
            shelter.Email = request.Email;
            shelter.ImageUrl = request.ImageUrl;
            shelter.PhoneNumber = request.PhoneNumber;
            shelter.CityId = request.CityId;

            userAccount.Username = request.Username;
            userAccount.Email = request.Email;
            userAccount.ImageUrl = request.ImageUrl;
            userAccount.PhoneNumber = request.PhoneNumber;
            userAccount.CityId = request.CityId;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                shelter.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);
                userAccount.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);
            }

            await db.SaveChangesAsync(cancellationToken);

            var response = new ShelterReadResponse
            {
                Id = shelter.Id,
                Name = shelter.Name,
                Owner = shelter.Owner,
                YearFounded = shelter.YearFounded,
                Address = shelter.Address,
                Username = shelter.Username,
                Email = shelter.Email,
                ImageUrl = shelter.ImageUrl,
                PhoneNumber = shelter.PhoneNumber,
                CityId = shelter.CityId,
                
            };

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShelter(int id, CancellationToken cancellationToken)
        {
            var shelter = await db.Shelter
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (shelter == null) return NotFound();

            var userAccount = await db.UserAccount
               .FirstOrDefaultAsync(x => x.Username == shelter.Username, cancellationToken);

            if (userAccount == null) return NotFound();

            db.Shelter.Remove(shelter);
            db.UserAccount.Remove(userAccount);
            await db.SaveChangesAsync(cancellationToken);

            return Ok(new { message = "Shelter deleted successfully." });
        }


        // Models for request and response
        public class ShelterReadResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Owner { get; set; }
            public int YearFounded { get; set; }
            public string Address { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string ImageUrl { get; set; }
            public string PhoneNumber { get; set; }
            public int CityId { get; set; }
            
        }

        public class ShelterCreateRequest
        {

            [MinLength(1, ErrorMessage = "Name must have at least 1 character."),
                MaxLength(100, ErrorMessage = "Name must be under 100 characters.")]
            public required string Name { get; set; }


            [MinLength(1, ErrorMessage = "Owner must have at least 1 character."),
                MaxLength(100, ErrorMessage = "Owner must be under 100 characters.")]
            public required string Owner { get; set; }


            [Range(1800, 2024, ErrorMessage = "YearFounded must be a valid year.")]
            public required int YearFounded { get; set; }


            [MinLength(1, ErrorMessage = "Adress must have at least 1 character."),
                MaxLength(100, ErrorMessage = "Address must be under 100 characters.")]
            public required string Address { get; set; }

            [MinLength(1, ErrorMessage = "Username must have at least 1 character."),
               MaxLength(20, ErrorMessage = "Username cannot be longer than 20 characters."),
               RegularExpression("^[a-zA-Z0-9_]*$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
            public required string Username { get; set; }

            
            [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
            //[RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one number.")]
            public required string Password { get; set; }



            [EmailAddress(ErrorMessage = "Email has to be in a valid format.")]
            public required string Email { get; set; }


            [RegularExpression(@"^(\+387|0)[6][0-7][0-9][0-9][0-9][0-9][0-9][0-9]$", ErrorMessage = "PhoneNumber must be a valid phone number for Bosnia and Herzegovina.")]
            public required string PhoneNumber { get; set; }

            public string ImageUrl { get; set; }


            public required int CityId { get; set; }
        }

        public class ShelterUpdateRequest
        {
            [MaxLength(50, ErrorMessage = "Name must be under 50 characters.")]
            public required string Name { get; set; }


            [MaxLength(50, ErrorMessage = "Owner must be under 50 characters.")]
            public required string Owner { get; set; }


            [Range(1800, 2023, ErrorMessage = "YearFounded must be a valid year.")]
            public required int YearFounded { get; set; }


            [MinLength(1, ErrorMessage = "Adress must have at least 1 character."),
                MaxLength(100, ErrorMessage = "Address must be under 100 characters.")]
            public required string Address { get; set; }

            [MinLength(1, ErrorMessage = "Username must have at least 1 character."),
               MaxLength(20, ErrorMessage = "Username cannot be longer than 20 characters."),
               RegularExpression("^[a-zA-Z0-9_]*$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
            public required string Username { get; set; }


            [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
            //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d\W_&&[^.]]{8,}$
//", ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one number.")]
            public required string Password { get; set; }


            [EmailAddress(ErrorMessage = "Email has to be in a valid format.")]
            public required string Email { get; set; }


            [RegularExpression(@"^(\+387|0)[6][0-7][0-9][0-9][0-9][0-9][0-9][0-9]$", ErrorMessage = "PhoneNumber must be a valid phone number for Bosnia and Herzegovina.")]
            public required string PhoneNumber { get; set; }

            public required string ImageUrl { get; set; }


            public required int CityId { get; set; }
        }

        [HttpGet("activate")]
        public async Task<IActionResult> ActivateShelter([FromQuery] string token, CancellationToken cancellationToken)
        {
            var shelter = await db.Shelter.FirstOrDefaultAsync(s => s.ActivationToken == token, cancellationToken);

            if (shelter == null)
            {
                return BadRequest("Nevaljan aktivacijski token.");
            }

            if (shelter.IsActivated)
            {
                return BadRequest("Račun je već aktiviran.");
            }

            shelter.IsActivated = true;
            shelter.ActivationToken = string.Empty; 
            await db.SaveChangesAsync(cancellationToken);

            return Ok("Vaš račun je uspješno aktiviran.");
        }

    }

}