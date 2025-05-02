using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;
using sapica_backend.Data.Models;
using sapica_backend.Endpoints.AdoptionPostEndpoints;
using sapica_backend.Helper.Api;
using System.Reflection.Metadata;
using System.Text;
using static sapica_backend.Endpoints.AdoptionRequestEndpoints.AdoptionRequestEndpoints;
using static System.Net.Mime.MediaTypeNames;

namespace sapica_backend.Endpoints.AdoptionRequestEndpoints
{
    [Route("requests")]
    public class GetAdoptionRequestsByUsernameEndpoint(ApplicationDbContext db) : MyEndpointBaseAsync.WithRequest<string>.WithActionResult<AdoptionRequestByUsernameReadResponse[]>
    {
        [HttpGet("{username}")]
        public async override Task<ActionResult<AdoptionRequestByUsernameReadResponse[]>> HandleAsync(string username, CancellationToken cancellationToken = default)
        {
            var adoptionRequests = await db.AdoptionRequest
                .Where(a => a.AdoptionPost.Username == username)
                .ToListAsync(cancellationToken);

            
            if (!adoptionRequests.Any())
            {
                return Ok(new List<AdoptionRequestByUsernameReadResponse>());
            }

            
            var responseList = new List<AdoptionRequestByUsernameReadResponse>();

            
            foreach (var request in adoptionRequests)
            {
                
                var adoptionPost = await db.AdoptionPost
                    .Where(ap => ap.Id == request.AdoptionPostId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (adoptionPost != null)
                {
                    
                    var animal = await db.Animal
                        .Where(a => a.Id == adoptionPost.AnimalId)
                        .FirstOrDefaultAsync(cancellationToken);

                    
                    var animalImage = await db.AnimalImage
                        .Where(ai => ai.AnimalId == animal.Id)
                        .FirstOrDefaultAsync(cancellationToken);


                    responseList.Add(new AdoptionRequestByUsernameReadResponse
                    {
                        Id = request.Id,
                        Date = request.Date,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Email = request.Email,
                        PhoneNumber = request.PhoneNumber,
                        Reason = request.Reason,
                        LivingSpace = request.LivingSpace,
                        Backyard = request.Backyard,
                        Age = request.Age,
                        FamilyMembers = request.FamilyMembers,
                        AnyKids = request.AnyKids,
                        AnyAnimalsBefore = request.AnyAnimalsBefore,
                        TimeCommitment = request.TimeCommitment,
                        PreferredCharacteristic = request.PreferredCharacteristic,
                        CityId = request.CityId,
                        City = request.City?.Name,
                        AdoptionPostId = request.AdoptionPostId,
                        AnimalName = animal?.Name,
                        AnimalType = animal?.AnimalType,
                        AnimalImage = animalImage != null ? Convert.ToBase64String(animalImage.Image) : null,
                        IsAccepted = request.IsAccepted
                    });
                }
            }

          
            return Ok(responseList.ToArray());
        }
    }

    public class AdoptionRequestByUsernameReadResponse
    {
       public int? Id { get; set; }
        public DateTime? Date { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Reason { get; set; }
        public string? LivingSpace { get; set; }
        public string? Backyard { get; set; }
        public int? Age { get; set; }

       

        public int? FamilyMembers { get; set; }
        public string? AnyKids { get; set; }
      
        public string? AnyAnimalsBefore { get; set; }
        
        
        public int? TimeCommitment { get; set; }
        public string? PreferredCharacteristic { get; set; }
        public int? AdoptionPostId { get; set; }
        public int? CityId { get; set; }
        public string? AnimalName { get; set; }
        public string? AnimalType { get; set; }
        public bool? IsAccepted { get; set; }
       
        public string? City { get; set; }
        public string? AnimalImage { get; set; }

    }
}

