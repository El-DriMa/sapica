using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;
using sapica_backend.Data.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Azure.Core;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using sapica_backend.Services;


namespace sapica_backend.Endpoints.AdoptionPostEndpoints
{
    [Route("adoptionPosts")]
    [ApiController]
    public class AdoptionPostEndpoints(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        [HttpGet("all")]
        public async Task<ActionResult<List<AdoptionPostReadResponse>>> GetAllAdoptionPosts(CancellationToken cancellationToken, int page = 1, int itemsPerPage = 10)
        {
     
            var adoptionPosts = await db.AdoptionPost
                .Include(x => x.Animal).ThenInclude(a => a.Images)
                .Include(x => x.City)
                .Skip((page - 1) * itemsPerPage) 
                .Take(itemsPerPage) 
                .ToListAsync(cancellationToken);

            var totalPosts = await db.AdoptionPost.CountAsync(cancellationToken); 

            var response = adoptionPosts.Select(x => new AdoptionPostReadResponse
            {
                Id = x.Id,
                DateOfCreation = x.DateOfCreation,
                DateOfModification = x.DateOfModification,
                ViewCounter = x.ViewCounter,
                Urgent = x.Urgent,
                ShortDescription = x.ShortDescription,
                Username = x.Username,
                CityId = x.CityId,
                City = x.City != null ? new CityReadResponse
                {
                    Id = x.City.Id,
                    Name = x.City.Name,
                    Latitude = x.City.Latitude,
                    Longitude = x.City.Longitude
                } : null,
                Animal = new AnimalReadResponse
                {
                    Id = x.Animal.Id,
                    Name = x.Animal.Name,
                    Gender = x.Animal.Gender,
                    Size = x.Animal.Size,
                    Age = x.Animal.Age,
                    Color = x.Animal.Color,
                    Weight = x.Animal.Weight,
                    AnimalType = x.Animal.AnimalType,
                    Vaccinated = x.Animal.Vaccinated,
                    Sterilized = x.Animal.Sterilized,
                    ParasiteFree = x.Animal.ParasiteFree,
                    HasPassport = x.Animal.HasPassport,
                    Images = x.Animal.Images.Select(image => new ImageReadResponse
                    {
                        AnimalId = x.Animal.Id,
                        Image = Convert.ToBase64String(image.Image)
                    }).ToList(),
                }
            }).ToList();

               var totalPages = (int)Math.Ceiling((double)totalPosts / itemsPerPage); 

             return Ok(new { adoptionPosts = response, totalPages = Math.Ceiling((double)totalPosts / itemsPerPage) });

        }

        [HttpGet("search")]
        public async Task<ActionResult<List<AdoptionPostReadResponse>>> SearchAdoptionPosts(
                            [FromQuery] bool? urgent = null,
                            [FromQuery] string? animalType = null,
                            [FromQuery] int? cityId = null,
                            [FromQuery] string? gender = null,
                            [FromQuery] string? size=null,
                            [FromQuery] int page = 1,
                            [FromQuery] int itemsPerPage = 10,
                            CancellationToken cancellationToken = default)
            {
            
            var query = db.AdoptionPost
                .Include(x => x.Animal).ThenInclude(a => a.Images)
                .Include(x => x.City)
                .AsQueryable();

            
            if (urgent.HasValue)
            {
                query = query.Where(x => x.Urgent == urgent.Value);
            }

            if (!string.IsNullOrEmpty(animalType))
            {
                query = query.Where(x => x.Animal.AnimalType == animalType);
            }

            if (cityId.HasValue && cityId.Value != 0) 
            {
                query = query.Where(x => x.CityId == cityId.Value);
            }

            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(x => x.Animal.Gender == gender);
            }

            if (!string.IsNullOrEmpty(size))
            {
                query = query.Where(x => x.Animal.Size == size);
            }

            
            var totalPosts = await query.CountAsync(cancellationToken);

            
            var adoptionPosts = await query
                .Skip((page - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync(cancellationToken);

            
            var response = adoptionPosts.Select(x => new AdoptionPostReadResponse
            {
                Id = x.Id,
                DateOfCreation = x.DateOfCreation,
                DateOfModification = x.DateOfModification,
                ViewCounter = x.ViewCounter,
                Urgent = x.Urgent,
                ShortDescription = x.ShortDescription,
                Username = x.Username,
                CityId = x.CityId,
                City = x.City != null ? new CityReadResponse
                {
                    Id = x.City.Id,
                    Name = x.City.Name,
                    Latitude = x.City.Latitude,
                    Longitude = x.City.Longitude
                } : null,
                Animal = new AnimalReadResponse
                {
                    Id = x.Animal.Id,
                    Name = x.Animal.Name,
                    Gender = x.Animal.Gender,
                    Size = x.Animal.Size,
                    Age = x.Animal.Age,
                    Color = x.Animal.Color,
                    Weight = x.Animal.Weight,
                    AnimalType = x.Animal.AnimalType,
                    Vaccinated = x.Animal.Vaccinated,
                    Sterilized = x.Animal.Sterilized,
                    ParasiteFree = x.Animal.ParasiteFree,
                    HasPassport = x.Animal.HasPassport,
                    Images = x.Animal.Images.Select(image => new ImageReadResponse
                    {
                        AnimalId = x.Animal.Id,
                        Image = Convert.ToBase64String(image.Image)
                    }).ToList(),
                }
            }).ToList();

            
            var totalPages = (int)Math.Ceiling((double)totalPosts / itemsPerPage);

            
            return Ok(new { adoptionPosts = response, totalPages });
        }



        [HttpGet("allLoggedIn")]
        public async Task<ActionResult<List<AdoptionPostReadResponse>>> GetAllAdoptionPostsLoggedInUser(CancellationToken cancellationToken)
        {
            var username = await GetUsernameFromCurrentUserAsync(cancellationToken);

            if (username == null)
            {
                return Unauthorized(new { message = "Unauthenticated." });
            }

            var adoptionPosts = await db.AdoptionPost
                .Where(x => x.Username == username)
                .Include(x => x.Animal).ThenInclude(a => a.Images)
                .Include(x => x.City)
                .ToListAsync(cancellationToken);

            var response = adoptionPosts.Select(x => new AdoptionPostReadResponse
            {
                Id = x.Id,
                DateOfCreation = x.DateOfCreation,
                DateOfModification = x.DateOfModification,
                ViewCounter = x.ViewCounter,
                Urgent = x.Urgent,
                ShortDescription = x.ShortDescription,
                Username = x.Username,
                CityId = x.CityId,
                City = x.City != null ? new CityReadResponse
                {
                    Id = x.City.Id,
                    Name = x.City.Name,
                    Latitude = x.City.Latitude,
                    Longitude = x.City.Longitude
                } : null,
                Animal = new AnimalReadResponse
                {
                    Id = x.Animal.Id,
                    Name = x.Animal.Name,
                    Gender = x.Animal.Gender,
                    Size = x.Animal.Size,
                    Age = x.Animal.Age,
                    Color = x.Animal.Color,
                    Weight = x.Animal.Weight,
                    AnimalType = x.Animal.AnimalType,
                    Vaccinated = x.Animal.Vaccinated,
                    Sterilized = x.Animal.Sterilized,
                    ParasiteFree = x.Animal.ParasiteFree,
                    HasPassport = x.Animal.HasPassport,
                    Images = x.Animal.Images.Select(image => new ImageReadResponse
                    {
                        AnimalId = x.Animal.Id,
                        Image = Convert.ToBase64String(image.Image)
                    }).ToList(),



                }
            }).ToList();


            return Ok(response);
        }




        [HttpPost("add")]
        public async Task<IActionResult> AddAdoptionPost([FromBody] AdoptionPostCreateRequest request, CancellationToken cancellationToken)
        {
            var username = await GetUsernameFromCurrentUserAsync(cancellationToken);

            if (username == null)
            {
                return Unauthorized(new { message = "Unauthenticated." });
            }

            if (request == null || request.Animal == null)
            {
                return BadRequest("Invalid request data.");
            }

            var animal = new Animal
            {
                Name = request.Animal.Name,
                Gender = request.Animal.Gender,
                Size = request.Animal.Size,
                Age = request.Animal.Age,
                Color = request.Animal.Color,
                Weight = request.Animal.Weight,
                AnimalType = request.Animal.AnimalType,
                Vaccinated = request.Animal.Vaccinated,
                Sterilized = request.Animal.Sterilized,
                ParasiteFree = request.Animal.ParasiteFree,
                HasPassport = request.Animal.HasPassport
            };

            db.Animal.Add(animal);
            await db.SaveChangesAsync(cancellationToken);

            if (request.Animal.Images != null && request.Animal.Images.Any())
            {
                var animalImages = request.Animal.Images
                    .Select(img =>
                    {
                        var base64String = img.Image.Contains(",") ? img.Image.Split(',')[1] : img.Image;
                        return new AnimalImage
                        {
                            AnimalId = animal.Id,
                            Image = Convert.FromBase64String(base64String)
                        };
                    }).ToList();

                db.AnimalImage.AddRange(animalImages);
                await db.SaveChangesAsync(cancellationToken);
            }

            var adoptionPost = new AdoptionPost
            {
                DateOfCreation = request.DateOfCreation,
                ViewCounter = request.ViewCounter,
                Urgent = request.Urgent,
                ShortDescription = request.ShortDescription,
                Username = username,
                AnimalId = animal.Id,
                CityId = request.CityId
            };

            db.AdoptionPost.Add(adoptionPost);
            await db.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetAdoptionPostById), new { id = adoptionPost.Id }, adoptionPost);
        }



        [HttpGet("adoptionpost/{id}")]
        public async Task<ActionResult<AdoptionPostReadResponse>> GetAdoptionPostById(int id, CancellationToken cancellationToken)
        {
            var adoptionPost = await db.AdoptionPost
                .Include(x => x.Animal)
                    .ThenInclude(a => a.Images).Include(x => x.City)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (adoptionPost == null)
            {
                return NotFound();
            }

            var response = new AdoptionPostReadResponse
            {
                Id = adoptionPost.Id,
                DateOfCreation = adoptionPost.DateOfCreation,
                DateOfModification = adoptionPost.DateOfModification,
                ViewCounter = adoptionPost.ViewCounter,
                Urgent = adoptionPost.Urgent,
                ShortDescription = adoptionPost.ShortDescription,
                Username = adoptionPost.Username,
                CityId = adoptionPost.CityId,
                City = adoptionPost.City != null ? new CityReadResponse
                {
                    Id = adoptionPost.City.Id,
                    Name = adoptionPost.City.Name,
                    Latitude = adoptionPost.City.Latitude,
                    Longitude = adoptionPost.City.Longitude
                } : null,
                Animal = new AnimalReadResponse
                {
                    Id = adoptionPost.Animal.Id,
                    Name = adoptionPost.Animal.Name,
                    Gender = adoptionPost.Animal.Gender,
                    Size = adoptionPost.Animal.Size,
                    Age = adoptionPost.Animal.Age,
                    Color = adoptionPost.Animal.Color,
                    Weight = adoptionPost.Animal.Weight,
                    AnimalType = adoptionPost.Animal.AnimalType,
                    Vaccinated = adoptionPost.Animal.Vaccinated,
                    Sterilized = adoptionPost.Animal.Sterilized,
                    ParasiteFree = adoptionPost.Animal.ParasiteFree,
                    HasPassport = adoptionPost.Animal.HasPassport,
                    Images = adoptionPost.Animal.Images.Select(img => new ImageReadResponse
                    {
                        AnimalId = adoptionPost.Animal.Id,
                        Image = img.Image != null ? Convert.ToBase64String(img.Image) : null
                    }).ToList()

                }
            };

            return Ok(response);
        }


        [HttpPut("update/{id}")]
        public async Task<ActionResult<AdoptionPostReadResponse>> UpdateAdoptionPost(int id, [FromBody] AdoptionPostUpdateRequest request, CancellationToken cancellationToken)
        {
            var username = await GetUsernameFromCurrentUserAsync(cancellationToken);
            if (username == null)
            {
                return Unauthorized(new { message = "Unauthenticated." });

            }

            var adoptionPost = await db.AdoptionPost
                .Include(x => x.Animal).ThenInclude(a => a.Images).Include(x => x.City)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (adoptionPost == null) return NotFound();

            if (adoptionPost.Username != username)
            {
                return Unauthorized(new { message = "You are not authorized to update this post." });
            }

            // Ažuriranje ostalih podataka
            if (request.ViewCounter.HasValue) adoptionPost.ViewCounter = request.ViewCounter.Value;
            if (request.Urgent.HasValue) adoptionPost.Urgent = request.Urgent.Value;
            if (request.ShortDescription != null) adoptionPost.ShortDescription = request.ShortDescription;
            if (request.Username != null) adoptionPost.Username = request.Username;
            if (request.CityId.HasValue) adoptionPost.CityId = request.CityId.Value;

            if (request.Animal != null)
            {
                var animal = adoptionPost.Animal;


                if (request.Animal.Name != null) animal.Name = request.Animal.Name!;
                if (request.Animal.Gender != null) animal.Gender = request.Animal.Gender;
                if (request.Animal.Size != null) animal.Size = request.Animal.Size;
                if (request.Animal.Age != null) animal.Age = request.Animal.Age;
                if (request.Animal.Color != null) animal.Color = request.Animal.Color;
                if (request.Animal.Weight.HasValue) animal.Weight = request.Animal.Weight.Value;
                if (request.Animal.AnimalType != null) animal.AnimalType = request.Animal.AnimalType;
                if (request.Animal.Vaccinated.HasValue) animal.Vaccinated = request.Animal.Vaccinated.Value;
                if (request.Animal.Sterilized.HasValue) animal.Sterilized = request.Animal.Sterilized.Value;
                if (request.Animal.ParasiteFree.HasValue) animal.ParasiteFree = request.Animal.ParasiteFree.Value;
                if (request.Animal.HasPassport.HasValue) animal.HasPassport = request.Animal.HasPassport.Value;


                if (request.Animal.Images != null)
                {
                    var newImageBase64Strings = request.Animal.Images
                        .Where(img => !string.IsNullOrEmpty(img.Image))
                        .Select(img => img.Image.Contains(",") ? img.Image.Split(',')[1] : img.Image)
                        .ToHashSet();

                    animal.Images = animal.Images
                        .Where(existingImage => newImageBase64Strings.Contains(Convert.ToBase64String(existingImage.Image)))
                        .ToList();

                    foreach (var image in request.Animal.Images)
                    {
                        if (!string.IsNullOrEmpty(image.Image))
                        {
                            var base64String = image.Image.Contains(",") ? image.Image.Split(',')[1] : image.Image;
                            var imageBytes = Convert.FromBase64String(base64String);

                            if (!animal.Images.Any(existingImage => existingImage.Image.SequenceEqual(imageBytes)))
                            {
                                animal.Images.Add(new AnimalImage { Image = imageBytes });
                            }
                        }
                    }
                }
            }

            await db.SaveChangesAsync(cancellationToken);

            
            var response = new AdoptionPostReadResponse
            {
                Id = adoptionPost.Id,
                DateOfCreation = adoptionPost.DateOfCreation,
                DateOfModification = adoptionPost.DateOfModification,
                ViewCounter = adoptionPost.ViewCounter,
                Urgent = adoptionPost.Urgent,
                ShortDescription = adoptionPost.ShortDescription,
                Username = adoptionPost.Username,
                CityId = adoptionPost.CityId,
                Animal = new AnimalReadResponse
                {
                    Id = adoptionPost.Animal.Id,
                    Name = adoptionPost.Animal.Name,
                    Gender = adoptionPost.Animal.Gender,
                    Size = adoptionPost.Animal.Size,
                    Age = adoptionPost.Animal.Age,
                    Color = adoptionPost.Animal.Color,
                    Weight = adoptionPost.Animal.Weight,
                    AnimalType = adoptionPost.Animal.AnimalType,
                    Vaccinated = adoptionPost.Animal.Vaccinated,
                    Sterilized = adoptionPost.Animal.Sterilized,
                    ParasiteFree = adoptionPost.Animal.ParasiteFree,
                    HasPassport = adoptionPost.Animal.HasPassport,
                    Images = adoptionPost.Animal.Images.Select(image => new ImageReadResponse
                    {
                        AnimalId = adoptionPost.Animal.Id,
                        Image = Convert.ToBase64String(image.Image)
                    }).ToList(),
                },
            };

            return Ok(response);
        }





        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAdoptionPost(int id, CancellationToken cancellationToken)
        {
            //var username = await GetUsernameFromCurrentUserAsync(cancellationToken);

            //if (username == null)
            //{
            //    return Unauthorized(new { message = "Unauthenticated." });
            //}

            var adoptionPost = await db.AdoptionPost
                .Include(x => x.Animal).ThenInclude(a => a.Images)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (adoptionPost == null) return NotFound();

            //if (adoptionPost.Username != username)
            //{
            //    return Unauthorized(new { message = "You are not authorized to delete this post." });
            //}

            var favourites = db.Favourite.Where(f => f.AdoptionPostId == id);
            db.Favourite.RemoveRange(favourites);

            if (adoptionPost.Animal?.Images?.Any() == true)
            {
                db.AnimalImage.RemoveRange(adoptionPost.Animal.Images);
            }

            if (adoptionPost.Animal != null)
            {
                db.Animal.Remove(adoptionPost.Animal);
            }

            db.AdoptionPost.Remove(adoptionPost);

            await db.SaveChangesAsync(cancellationToken);

            return NoContent();
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
      

    public class AdoptionPostReadResponse
    {
        public int Id { get; set; }
        public DateOnly DateOfCreation { get; set; }
        public DateOnly? DateOfModification { get; set; }
        public int ViewCounter { get; set; }
        public bool Urgent { get; set; }
        public string ShortDescription { get; set; }
        public string Username { get; set; }=string.Empty;
        public AnimalReadResponse Animal { get; set; }
        public int? CityId { get; set; }
        public CityReadResponse? City { get; set; }
    }

    public class AdoptionPostCreateRequest
    {
        [Required]
        public DateOnly DateOfCreation { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "ViewCounter must be a positive number.")]
        public int ViewCounter { get; set; }

        public bool Urgent { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "ShortDescription must be between 2 and 500 characters.")]
        public string ShortDescription { get; set; }

        public string Username { get; set; }=string.Empty;

        [Required]
        public AnimalCreateRequest Animal { get; set; }
        [Required]
        public int? CityId { get; set; }
    }

    public class AdoptionPostUpdateRequest
    {
        public DateOnly? DateOfCreation { get; set; }

        public DateOnly? DateOfModification { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "ViewCounter must be a positive number.")]
        public int? ViewCounter { get; set; }

        public bool? Urgent { get; set; }

        [StringLength(500, MinimumLength = 2, ErrorMessage = "ShortDescription must be between 2 and 500 characters.")]
        public string? ShortDescription { get; set; }

        public string? Username { get; set; }

        public AnimalUpdateRequest? Animal { get; set; }
        public int? CityId { get; set; }
    }


    public class AnimalReadResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Size { get; set; }
        public string Age { get; set; }
        public string Color { get; set; }
        public double Weight { get; set; }
        public string AnimalType { get; set; }
        public bool Vaccinated { get; set; }
        public bool Sterilized { get; set; }
        public bool ParasiteFree { get; set; }
        public bool HasPassport { get; set; }
        public List<ImageReadResponse> Images { get; set; }

    }

    public class AnimalCreateRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 i 50 characters.")]
        public string Name { get; set; }

        [Required]
        [RegularExpression("Mužjak|Ženka", ErrorMessage = "Gender must be Mužjak ili Ženka.")]
        public string Gender { get; set; }

        [Required]
        [RegularExpression("Malo|Srednje|Veliko", ErrorMessage = "Size must be Malo Srednje ili Veliko.")]
        public string Size { get; set; }

        [Required]
        public string Age { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Color must be between 2 i 50 characters.")]
        public string Color { get; set; }

        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Weight must be greater than 0.")]
        public double Weight { get; set; }

        [Required]
        [RegularExpression("^(Pas|Mačka|Ptica|Zec|Ostalo)$", ErrorMessage = "Animal type must be Pas, Mačka, Ptica, Zec or Ostalo.")]
        public string AnimalType { get; set; }

        public bool Vaccinated { get; set; }
        public bool Sterilized { get; set; }
        public bool ParasiteFree { get; set; }
        public bool HasPassport { get; set; }
        public List<ImageCreateRequest> Images { get; set; }

    }

    public class AnimalUpdateRequest
    {
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 i 50 characters.")]
        public string? Name { get; set; }

        [RegularExpression("Mužjak|Ženka", ErrorMessage = "Gender must be Mužjak ili Ženka.")]
        public string? Gender { get; set; }

        [RegularExpression("Malo|Srednje|Veliko", ErrorMessage = "Size must be Malo Srednje ili Veliko.")]
        public string? Size { get; set; }

        public string? Age { get; set; }

        [StringLength(50, MinimumLength = 2, ErrorMessage = "Color must be between 2 i 50 characters.")]
        public string? Color { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Weight must be greater than 0.")]
        public double? Weight { get; set; }

        [RegularExpression("^(Pas|Mačka|Ptica|Zec|Ostalo)$", ErrorMessage = "Animal type must be Pas, Mačka, Ptica, Zec or Ostalo.")]
        public string? AnimalType { get; set; }

        public bool? Vaccinated { get; set; }
        public bool? Sterilized { get; set; }
        public bool? ParasiteFree { get; set; }
        public bool? HasPassport { get; set; }
        public List<ImageReadResponse> Images { get; set; }

    }

    public class CityReadResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class ImageReadResponse
    {
        public string Image { get; set; }
        public int AnimalId { get; set; }
    }

    public class ImageCreateRequest
    {
        public required string Image { get; set; }
        public required int AnimalId { get; set; }
    }
    public class ImageUpdateRequest
    {
        public string? Image { get; set; }
        public int? AnimalId { get; set; }
    }
}

