using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;
using sapica_backend.Data.Models;

namespace sapica_backend.Endpoints.SponsorEndpoints
{
    [Route("sponsors")]
    [ApiController]
    public class SponsorEndpoints(ApplicationDbContext dbContext) : ControllerBase
    {
        [HttpPut("generate-sponsors")]
        public async Task<string> GenerateSponsors(CancellationToken cancellationToken = default)
        {
            var sponsors = new List<Sponsor>()
            {
                new Sponsor
                {
                    Name = "Purina",
                    LogoUrl = "/Images/Purina_logo_PNG1.png",
                    WebsiteUrl = "https://www.purina.com/",
                    Description = "Vrhunskа hrana za kućne ljubimce koju vole vaši ljubimci.",
                    ContactEmail = "info@purina.com",
                    Address = "St. Louis, Missouri, USA"
                },
                new Sponsor
                {
                    Name = "Royal Canin",
                    LogoUrl = "/Images/Royal_Canin_logo_PNG1.png",
                    WebsiteUrl = "https://www.royalcanin.com/us",
                    Description = "Prilagođena ishrana za potrebe vaših ljubimaca.",
                    ContactEmail = "support@royalcanin.com",
                    Address = "Aimargues, France"
                },
                new Sponsor
                {
                    Name = "Pedigree",
                    LogoUrl = "/Images/Pedigree_logo_PNG6.png",
                    WebsiteUrl = "https://www.pedigree.com/",
                    Description = "Zdrava hrana za sretne pse.",
                    ContactEmail = "contact@pedigree.com",
                    Address = "McLean, Virginia, USA"
                },
                new Sponsor
                {
                    Name = "Hill's Science Diet",
                    LogoUrl = "/Images/Hills_logo_PNG1.png",
                    WebsiteUrl = "https://www.hillspet.com/",
                    Description = "Zdravlje vaših ljubimaca kroz naučno razvijenu ishranu.",
                    ContactEmail = "help@hillspet.com",
                    Address = "Topeka, Kansas, USA"
                },
                new Sponsor
                {
                    Name = "Whiskas",
                    LogoUrl = "/Images/Whiskas_logo_PNG7.png",
                    WebsiteUrl = "https://www.whiskas.co.uk/",
                    Description = "Ukusna hrana za svaku mačku.",
                    ContactEmail = "care@whiskas.com",
                    Address = "McLean, Virginia, USA"
                },
                new Sponsor
                {
                    Name = "KONG",
                    LogoUrl = "/Images/kong_logo.png",
                    WebsiteUrl = "https://www.kongcompany.com/",
                    Description = "Inovativne igračke za aktivne ljubimce.",
                    ContactEmail = "info@kongcompany.com",
                    Address = "Golden, Colorado, USA"
                },
                new Sponsor
                {
                    Name = "Chewy",
                    LogoUrl = "/Images/Chewy-Logo.png",
                    WebsiteUrl = "https://www.chewy.com/",
                    Description = "Sve što vaš ljubimac treba, dostavljeno.",
                    ContactEmail = "service@chewy.com",
                    Address = "Dania Beach, Florida, USA"
                },
                new Sponsor
                {
                    Name = "PetSafe",
                    LogoUrl = "/Images/petsafe.jpg",
                    WebsiteUrl = "https://www.petsafe.com/",
                    Description = "Sigurni i inovativni proizvodi za ljubimce.",
                    ContactEmail = "support@petsafe.com",
                    Address = "Knoxville, Tennessee, USA"
                },
                new Sponsor
                {
                    Name = "Blue Buffalo",
                    LogoUrl = "/Images/Blue_Buffalo_logo_PNG1.png",
                    WebsiteUrl = "https://bluebuffalo.com/",
                    Description = "Zdrava i prirodna hrana za ljubimce.",
                    ContactEmail = "info@bluebuffalo.com",
                    Address = "Wilton, Connecticut, USA"
                },
                new Sponsor
                {
                    Name = "Nylabone",
                    LogoUrl = "/Images/nylabone.jpg",
                    WebsiteUrl = "https://www.nylabone.com/",
                    Description = "Zabavne igračke i poslastice za pse.",
                    ContactEmail = "contact@nylabone.com",
                    Address = "Neptune City, New Jersey, USA"
                }
                
             };
            await dbContext.Sponsor.AddRangeAsync(sponsors,cancellationToken);
            await dbContext.SaveChangesAsync();

            return "Sponsors generated successfully.";

        }
        [HttpGet("all")]
        public Task<List<SponsorReadResponse>> GetAllSponsors(CancellationToken cancellationToken = default)
        {
            return dbContext.Sponsor.Select(s => new SponsorReadResponse
            {
                Id = s.Id,
                Name = s.Name,
                LogoUrl = s.LogoUrl,
                WebsiteUrl = s.WebsiteUrl,
                Description = s.Description,
                ContactEmail = s.ContactEmail,
                Address = s.Address
            }).ToListAsync(cancellationToken);
        }
    }
    public class SponsorReadResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }=string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string WebsiteUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
