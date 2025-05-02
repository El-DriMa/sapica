using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;
using sapica_backend.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace sapica_backend.Endpoints.AdoptionRequestEndpoints
{
    [Route("adoption-requests")]
    [ApiController]
    public class AdoptionRequestEndpoints(ApplicationDbContext db) : ControllerBase
    {
        // GET: /adoption-requests
        [HttpGet("")]
        public async Task<AdoptionRequestReadResponse[]> GetAllAdoptionRequests(CancellationToken cancellationToken)
        {
            var response = await db.AdoptionRequest
                .Select(ar => new AdoptionRequestReadResponse
                {
                    Id = ar.Id,
                    Date = ar.Date,
                    FirstName = ar.FirstName,
                    LastName = ar.LastName,
                    Email = ar.Email,
                    PhoneNumber = ar.PhoneNumber,
                    Reason = ar.Reason,
                    LivingSpace = ar.LivingSpace,
                    Backyard = ar.Backyard,
                    Age = ar.Age,
                    BackyardSize = ar.BackyardSize,
                    FamilyMembers = ar.FamilyMembers,
                    AnyKids = ar.AnyKids,
                    AnimalsBefore = ar.AnimalsBefore,
                    AnyAnimalsBefore = ar.AnyAnimalsBefore,
                    Experience = ar.Experience,
                    NumberOfKids = ar.NumberOfKids,
                    PreferredCharacteristic = ar.PreferredCharacteristic,
                    TimeCommitment = ar.TimeCommitment,
                    AdoptionPostId = ar.AdoptionPostId,
                    CityId = ar.CityId,

                    UserId = ar.UserId
                })
                .ToArrayAsync(cancellationToken);

            return response;
        }

        // GET: /adoption-requests/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AdoptionRequestReadResponse>> GetAdoptionRequestById(int id, CancellationToken cancellationToken)
        {
            var adoptionRequest = await db.AdoptionRequest
                .Include(ar => ar.City)
                .FirstOrDefaultAsync(ar => ar.Id == id, cancellationToken);

            if (adoptionRequest == null) return NotFound();

            var response = new AdoptionRequestReadResponse
            {
                Id = adoptionRequest.Id,
                Date = adoptionRequest.Date,
                FirstName = adoptionRequest.FirstName,
                LastName = adoptionRequest.LastName,
                Email = adoptionRequest.Email,
                PhoneNumber = adoptionRequest.PhoneNumber,
                Reason = adoptionRequest.Reason,
                LivingSpace = adoptionRequest.LivingSpace,
                Backyard = adoptionRequest.Backyard,
                Age = adoptionRequest.Age,
                BackyardSize = adoptionRequest.BackyardSize,
                FamilyMembers = adoptionRequest.FamilyMembers,
                AnyKids = adoptionRequest.AnyKids,
                AnimalsBefore = adoptionRequest.AnimalsBefore,
                AnyAnimalsBefore = adoptionRequest.AnyAnimalsBefore,
                Experience = adoptionRequest.Experience,
                NumberOfKids = adoptionRequest.NumberOfKids,
                PreferredCharacteristic = adoptionRequest.PreferredCharacteristic,
                TimeCommitment = adoptionRequest.TimeCommitment,
                AdoptionPostId = adoptionRequest.AdoptionPostId,
                CityId = adoptionRequest.CityId,
                UserId = adoptionRequest.UserId
            };

            return Ok(response);
        }

        // POST: /adoption-requests
        [HttpPost("")]
        public async Task<IActionResult> AddAdoptionRequest([FromBody] AdoptionRequestCreateRequest request, CancellationToken cancellationToken)
        {
            var adoptionPostExists = await db.AdoptionPost.AnyAsync(ap => ap.Id == request.AdoptionPostId, cancellationToken);
            var cityExists = await db.City.AnyAsync(c => c.Id == request.CityId, cancellationToken);

            if (!adoptionPostExists || !cityExists)
            {
                return BadRequest(new { message = "Provided AdoptionPostId or CityId does not exist in the database." });
            }

            var adoptionRequest = new AdoptionRequest
            {
                Date = request.Date,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Reason = request.Reason,
                LivingSpace = request.LivingSpace,
                Backyard = request.Backyard,
                Age = request.Age,
                BackyardSize = request.BackyardSize,
                FamilyMembers = request.FamilyMembers,
                AnyKids = request.AnyKids,
                AnimalsBefore = request.AnimalsBefore,
                AnyAnimalsBefore = request.AnyAnimalsBefore,
                Experience = request.Experience,
                NumberOfKids = request.NumberOfKids,
                PreferredCharacteristic = request.PreferredCharacteristic,
                TimeCommitment = request.TimeCommitment,
                AdoptionPostId = request.AdoptionPostId,
                CityId = request.CityId,
                UserId = request.UserId
            };

            db.AdoptionRequest.Add(adoptionRequest);
            await db.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetAdoptionRequestById), new { id = adoptionRequest.Id }, adoptionRequest);
        }

        // PUT: /adoption-requests/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdoptionRequest(int id, [FromBody] AdoptionRequestUpdateRequest request, CancellationToken cancellationToken)
        {
            var adoptionRequest = await db.AdoptionRequest
                .FirstOrDefaultAsync(ar => ar.Id == id, cancellationToken);

            if (adoptionRequest == null) return NotFound();

            var adoptionPostExists = await db.AdoptionPost.AnyAsync(ap => ap.Id == request.AdoptionPostId, cancellationToken);
            var cityExists = await db.City.AnyAsync(c => c.Id == request.CityId, cancellationToken);

            if (!adoptionPostExists || !cityExists)
            {
                return BadRequest(new { message = "Provided AdoptionPostId or CityId does not exist in the database." });
            }

            adoptionRequest.Date = request.Date;
            adoptionRequest.FirstName = request.FirstName;
            adoptionRequest.LastName = request.LastName;
            adoptionRequest.Email = request.Email;
            adoptionRequest.PhoneNumber = request.PhoneNumber;
            adoptionRequest.Reason = request.Reason;
            adoptionRequest.LivingSpace = request.LivingSpace;
            adoptionRequest.Backyard = request.Backyard;
            adoptionRequest.Age = request.Age;
            adoptionRequest.BackyardSize = request.BackyardSize;
            adoptionRequest.FamilyMembers = request.FamilyMembers;
            adoptionRequest.AnyKids = request.AnyKids;
            adoptionRequest.NumberOfKids = request.NumberOfKids;
            adoptionRequest.AnyAnimalsBefore = request.AnyAnimalsBefore;
            adoptionRequest.AnimalsBefore = request.AnimalsBefore;
            adoptionRequest.Experience = request.Experience;
            adoptionRequest.PreferredCharacteristic = request.PreferredCharacteristic;
            adoptionRequest.TimeCommitment = request.TimeCommitment;
            adoptionRequest.AdoptionPostId = request.AdoptionPostId;
            adoptionRequest.CityId = request.CityId;
            adoptionRequest.UserId = request.UserId;

            await db.SaveChangesAsync(cancellationToken);

            var response = new AdoptionRequestReadResponse
            {
                Id = adoptionRequest.Id,
                Date = adoptionRequest.Date,
                FirstName = adoptionRequest.FirstName,
                LastName = adoptionRequest.LastName,
                Email = adoptionRequest.Email,
                PhoneNumber = adoptionRequest.PhoneNumber,
                Reason = adoptionRequest.Reason,
                LivingSpace = adoptionRequest.LivingSpace,
                Backyard = adoptionRequest.Backyard,
                Age = adoptionRequest.Age,
                BackyardSize = adoptionRequest.BackyardSize,
                FamilyMembers = adoptionRequest.FamilyMembers,
                AnyKids = adoptionRequest.AnyKids,
                AnimalsBefore = adoptionRequest.AnimalsBefore,
                AnyAnimalsBefore = adoptionRequest.AnyAnimalsBefore,
                Experience = adoptionRequest.Experience,
                NumberOfKids = adoptionRequest.NumberOfKids,
                PreferredCharacteristic = adoptionRequest.PreferredCharacteristic,
                TimeCommitment = adoptionRequest.TimeCommitment,
                AdoptionPostId = adoptionRequest.AdoptionPostId,
                CityId = adoptionRequest.CityId,
                UserId = adoptionRequest.UserId
            };

            return Ok(response);
        }

        // DELETE: /adoption-requests/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdoptionRequest(int id, CancellationToken cancellationToken)
        {
            var adoptionRequest = await db.AdoptionRequest
                .FirstOrDefaultAsync(ar => ar.Id == id, cancellationToken);

            if (adoptionRequest == null) return NotFound();

            db.AdoptionRequest.Remove(adoptionRequest);
            await db.SaveChangesAsync(cancellationToken);

            return Ok(new { message = "Adoption request deleted successfully." });
        }

        // Models for request and response
        public class AdoptionRequestReadResponse
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Reason { get; set; }
            public string LivingSpace { get; set; }
            public string Backyard { get; set; }
            public int Age { get; set; }

            public string? BackyardSize { get; set; }

            public int FamilyMembers { get; set; }
            public string AnyKids { get; set; }
            public int? NumberOfKids { get; set; }
            public string AnyAnimalsBefore { get; set; }
            public string? AnimalsBefore { get; set; }
            public string? Experience { get; set; }
            public int TimeCommitment { get; set; }
            public string PreferredCharacteristic { get; set; }
            public int AdoptionPostId { get; set; }
            public int CityId { get; set; }

            public int? UserId { get; set; }
            public string? City {  get; set; }
            public string? AnimalImage { get; set; }
           
        }

        public class AdoptionRequestCreateRequest
        {
            public DateTime Date { get; set; } = DateTime.Now;

            [MinLength(1, ErrorMessage = "Name must have at least 1 character."),
                MaxLength(50, ErrorMessage = "Name must be under 50 characters.")]
            public required string FirstName { get; set; }
            [MinLength(1, ErrorMessage = "Last name must have at least 1 character."),
                MaxLength(50, ErrorMessage = "Last name must be under 50 characters.")]
            public required string LastName { get; set; }


            [EmailAddress(ErrorMessage = "Email has to be in a valid format.")]
            public required string Email { get; set; }

            [RegularExpression(@"^(\+387|0)[6][0-7][0-9][0-9][0-9][0-9][0-9][0-9]$", ErrorMessage = "PhoneNumber must be a valid phone number for Bosnia and Herzegovina.")]
            public required string PhoneNumber { get; set; }

            [MinLength(1, ErrorMessage = "Reason must have at least 1 character."),
                MaxLength(255, ErrorMessage = "Reason must be under 255 characters.")]
            public required string Reason { get; set; }



            [MinLength(1, ErrorMessage = "Living space must have at least 1 character."),
                MaxLength(20, ErrorMessage = "Living space must be under 20 characters.")]
            public required string LivingSpace { get; set; }


            public required string Backyard { get; set; }


            [Range(18, 100, ErrorMessage = "Age must be between 18 and 100.")]
            public required int Age { get; set; }

            public string? BackyardSize { get; set; }

            [Range(0, 30, ErrorMessage = "There must be less than 30 family members.")]
            public required int FamilyMembers { get; set; }

            public required string AnyKids { get; set; }
            [Range(0, 30, ErrorMessage = "Number of kids must be less than 30")]
            public int? NumberOfKids { get; set; }
            public required string AnyAnimalsBefore { get; set; }
            public string? AnimalsBefore { get; set; }
            public string? Experience { get; set; }

            [Range(0, 24, ErrorMessage = "Time of commitment must be between 0 and 24.")]
            public required int TimeCommitment { get; set; }
            [MinLength(1, ErrorMessage = "PreferredCharacteristic must have at least 1 character."),
               MaxLength(255, ErrorMessage = "PrefferedCharacteristics  must be under 255 characters.")]
            public required string PreferredCharacteristic { get; set; }

            [Required]
            public required int AdoptionPostId { get; set; }
            [Required]
            public required int CityId { get; set; }
            public int? UserId { get; set; }
        }

        public class AdoptionRequestUpdateRequest
        {
            public DateTime Date { get; set; } = DateTime.Now;

            [MinLength(1, ErrorMessage = "Name must have at least 1 character."),
                MaxLength(50, ErrorMessage = "Name must be under 50 characters.")]
            public required string FirstName { get; set; }

            [RegularExpression(@"^(\+387|0)[6][0-7][0-9][0-9][0-9][0-9][0-9][0-9]$", ErrorMessage = "PhoneNumber must be a valid phone number for Bosnia and Herzegovina.")]
            public required string PhoneNumber { get; set; }

            [MinLength(1, ErrorMessage = "Last Name must have at least 1 character."),
                MaxLength(50, ErrorMessage = "Last name must be under 50 characters.")]
            public required string LastName { get; set; }
            [EmailAddress(ErrorMessage = "Email has to be in a valid format.")]
            public required string Email { get; set; }

            [MinLength(1, ErrorMessage = "Reason must have at least 1 character."),
                MaxLength(100, ErrorMessage = "Reason must be under 100 characters.")]
            public required string Reason { get; set; }

            [MinLength(1, ErrorMessage = "Living space must have at least 1 character."),
                MaxLength(20, ErrorMessage = "Lving space  must be under 20 characters.")]
            public required string LivingSpace { get; set; }


            public required string Backyard { get; set; }

            [Range(18, 100, ErrorMessage = "Age must be between 18 and 100.")]
            public required int Age { get; set; }

            public string? BackyardSize { get; set; }

            [Range(0, 30, ErrorMessage = "Clanova mora biti manje od 30.")]
            public required int FamilyMembers { get; set; }

            public required string AnyKids { get; set; }
            [Range(0, 30, ErrorMessage = "Djece mora biti manje od 30.")]
            public int? NumberOfKids { get; set; }
            public required string AnyAnimalsBefore { get; set; }
            public string? AnimalsBefore { get; set; }
            public string? Experience { get; set; }
            [Range(0, 24, ErrorMessage = "Posveceno vrijeme mora biti izmedju 0 i 24 sata.")]
            public required int TimeCommitment { get; set; }
            [MinLength(1, ErrorMessage = "PreferredCharacteristic must have at least 1 character."),
               MaxLength(50, ErrorMessage = "PrefferedCharacteristics  must be under 50 characters.")]
            public required string PreferredCharacteristic { get; set; }
            [Required]
            public required int AdoptionPostId { get; set; }
            [Required]
            public required int CityId { get; set; }
            public int? UserId { get; set; }
        }
    }
}





