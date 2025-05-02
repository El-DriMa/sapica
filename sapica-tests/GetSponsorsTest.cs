using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;
using sapica_backend.Data.Models;
using sapica_backend.Endpoints.SponsorEndpoints;
using Xunit;

namespace sapica_tests
{
    public class GetSponsorsTest
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly SponsorEndpoints _sponsorEndpoints;

        public GetSponsorsTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new ApplicationDbContext(options);

            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();
            SeedDatabase();

            _sponsorEndpoints = new SponsorEndpoints(_dbContext);
        }

        private void SeedDatabase()
        {
            var sponsors = new List<Sponsor>
            {
                new Sponsor
                {
                    Id = 1,
                    Name = "Purina",
                    LogoUrl = "/Images/Purina_logo_PNG1.png",
                    WebsiteUrl = "https://www.purina.com/",
                    Description = "Vrhunskа hrana za kućne ljubimce koju vole vaši ljubimci.",
                    ContactEmail = "info@purina.com",
                    Address = "St. Louis, Missouri, USA"
                },
                new Sponsor
                {
                    Id = 2,
                    Name = "Royal Canin",
                    LogoUrl = "/Images/Royal_Canin_logo_PNG1.png",
                    WebsiteUrl = "https://www.royalcanin.com/us",
                    Description = "Prilagođena ishrana za potrebe vaših ljubimaca.",
                    ContactEmail = "support@royalcanin.com",
                    Address = "Aimargues, France"
                }
            };

            _dbContext.Sponsor.AddRange(sponsors);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task GetAllSponsors_ShouldReturnSponsorsIfAny()
        {
            var result = await _sponsorEndpoints.GetAllSponsors();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Purina", result[0].Name);
            Assert.Equal("Royal Canin", result[1].Name);
        }

        [Fact]
        public async Task GetAllSponsors_ShouldReturnEmptyListIfNone()
        {
            _dbContext.Sponsor.RemoveRange(_dbContext.Sponsor);
            await _dbContext.SaveChangesAsync();

            var result = await _sponsorEndpoints.GetAllSponsors();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllSponsors_ShouldReturnCorrectSponsorDetails()
        {
            var result = await _sponsorEndpoints.GetAllSponsors();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var purina = result.FirstOrDefault(s => s.Name == "Purina");
            Assert.NotNull(purina);
            Assert.Equal("/Images/Purina_logo_PNG1.png", purina.LogoUrl);
            Assert.Equal("https://www.purina.com/", purina.WebsiteUrl);
            Assert.Equal("Vrhunskа hrana za kućne ljubimce koju vole vaši ljubimci.", purina.Description);
            Assert.Equal("info@purina.com", purina.ContactEmail);
            Assert.Equal("St. Louis, Missouri, USA", purina.Address);

            var royalCanin = result.FirstOrDefault(s => s.Name == "Royal Canin");
            Assert.NotNull(royalCanin);
            Assert.Equal("/Images/Royal_Canin_logo_PNG1.png", royalCanin.LogoUrl);
            Assert.Equal("https://www.royalcanin.com/us", royalCanin.WebsiteUrl);
            Assert.Equal("Prilagođena ishrana za potrebe vaših ljubimaca.", royalCanin.Description);
            Assert.Equal("support@royalcanin.com", royalCanin.ContactEmail);
            Assert.Equal("Aimargues, France", royalCanin.Address);
        }
    }
}