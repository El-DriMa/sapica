using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;
using sapica_backend.Data.Models;
using sapica_backend.Endpoints;
using sapica_backend.Endpoints.AdoptionPostEndpoints;
using Xunit;

namespace sapica_tests
{
    public class GetAdoptionPostsTest : IAsyncLifetime
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly AdoptionPostEndpoints _adoptionPostEndpoints;

        public GetAdoptionPostsTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _adoptionPostEndpoints = new AdoptionPostEndpoints(_dbContext, null);
        }

        public async Task InitializeAsync()
        {
            // Obriši bazu podataka ako postoji
            await _dbContext.Database.EnsureDeletedAsync();

            // Kreiraj bazu podataka sa svim tabelama
            await _dbContext.Database.EnsureCreatedAsync();

            // Seedovanje podataka
            var adoptionPosts = new List<AdoptionPost>
            {
                new AdoptionPost
                {
                    Id = 1,
                    DateOfCreation = DateOnly.FromDateTime(DateTime.UtcNow),
                    DateOfModification = DateOnly.FromDateTime(DateTime.UtcNow),
                    ViewCounter = 10,
                    Urgent = true,
                    ShortDescription = "Test Post 1",
                    Username = "user1",
                    CityId = 1,
                    City = new City { Id = 1, Name = "City1", Latitude = 10.0, Longitude = 20.0, PostalCode = "22222" },
                    Animal = new Animal
                    {
                        Id = 1,
                        Name = "Buddy",
                        Gender = "Male",
                        Size = "Medium",
                        Age = "2",
                        Color = "Brown",
                        Weight = 15.5,
                        AnimalType = "Dog",
                        Vaccinated = true,
                        Sterilized = true,
                        ParasiteFree = true,
                        HasPassport = true,
                        Images = new List<AnimalImage>
                        {
                            new AnimalImage { AnimalId = 1, Image = new byte[] { 0x12, 0x34, 0x56 } }
                        }
                    }
                },
                new AdoptionPost
                {
                    Id = 2,
                    DateOfCreation = DateOnly.FromDateTime(DateTime.UtcNow),
                    DateOfModification = DateOnly.FromDateTime(DateTime.UtcNow),
                    ViewCounter = 5,
                    Urgent = false,
                    ShortDescription = "Test Post 2",
                    Username = "user2",
                    CityId = 2,
                    City = new City { Id = 2, Name = "City2", Latitude = 30.0, Longitude = 40.0, PostalCode = "22222" },
                    Animal = new Animal
                    {
                        Id = 2,
                        Name = "Milo",
                        Gender = "Female",
                        Size = "Small",
                        Age = "1",
                        Color = "Black",
                        Weight = 10.0,
                        AnimalType = "Cat",
                        Vaccinated = true,
                        Sterilized = false,
                        ParasiteFree = true,
                        HasPassport = false,
                        Images = new List<AnimalImage>
                        {
                            new AnimalImage { AnimalId = 2, Image = new byte[] { 0x78, 0x9A, 0xBC } }
                        }
                    }
                }
            };

            await _dbContext.AdoptionPost.AddRangeAsync(adoptionPosts);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
           
            await _dbContext.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task GetAllAdoptionPosts_ShouldReturnPaginatedResults()
        {
            var cancellationToken = CancellationToken.None;
            int page = 1;
            int itemsPerPage = 10;

            var result = await _adoptionPostEndpoints.GetAllAdoptionPosts(cancellationToken, page, itemsPerPage);

            var actionResult = Assert.IsType<ActionResult<List<AdoptionPostReadResponse>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

            var response = okResult.Value;
            var adoptionPostsProperty = response.GetType().GetProperty("adoptionPosts");
            var totalPagesProperty = response.GetType().GetProperty("totalPages");

            Assert.NotNull(adoptionPostsProperty);
            Assert.NotNull(totalPagesProperty);

            var returnedPosts = adoptionPostsProperty.GetValue(response) as List<AdoptionPostReadResponse>;
            var totalPages = (double)totalPagesProperty.GetValue(response);

            Assert.NotNull(returnedPosts);
            Assert.Equal(2, returnedPosts.Count);
            Assert.Equal(1, totalPages);
        }

        [Fact]
        public async Task GetAllAdoptionPosts_ShouldReturnEmptyListIfNoPosts()
        {
           
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();

            var cancellationToken = CancellationToken.None;
            int page = 1;
            int itemsPerPage = 10;

            var result = await _adoptionPostEndpoints.GetAllAdoptionPosts(cancellationToken, page, itemsPerPage);

            var actionResult = Assert.IsType<ActionResult<List<AdoptionPostReadResponse>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

            var response = okResult.Value;
            var adoptionPostsProperty = response.GetType().GetProperty("adoptionPosts");
            var totalPagesProperty = response.GetType().GetProperty("totalPages");

            Assert.NotNull(adoptionPostsProperty);
            Assert.NotNull(totalPagesProperty);

            var returnedPosts = adoptionPostsProperty.GetValue(response) as List<AdoptionPostReadResponse>;
            var totalPages = (double)totalPagesProperty.GetValue(response);

            Assert.NotNull(returnedPosts);
            Assert.Empty(returnedPosts);
            Assert.Equal(0, totalPages);
        }

        [Fact]
        public async Task GetAllAdoptionPosts_ShouldReturnCorrectPage()
        {
            var cancellationToken = CancellationToken.None;
            int page = 2; 
            int itemsPerPage = 1;

            var result = await _adoptionPostEndpoints.GetAllAdoptionPosts(cancellationToken, page, itemsPerPage);

            var actionResult = Assert.IsType<ActionResult<List<AdoptionPostReadResponse>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

            var response = okResult.Value;
            var adoptionPostsProperty = response.GetType().GetProperty("adoptionPosts");
            var totalPagesProperty = response.GetType().GetProperty("totalPages");

            Assert.NotNull(adoptionPostsProperty);
            Assert.NotNull(totalPagesProperty);

            var returnedPosts = adoptionPostsProperty.GetValue(response) as List<AdoptionPostReadResponse>;
            var totalPages = (double)totalPagesProperty.GetValue(response);

            Assert.NotNull(returnedPosts);
            Assert.Single(returnedPosts); 
            Assert.Equal(2, totalPages); 
        }
    }
}