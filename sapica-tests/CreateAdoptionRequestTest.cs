using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using sapica_backend.Data;
using sapica_backend.Data.Models;
using sapica_backend.Endpoints;
using FluentValidation.TestHelper;
using sapica_backend.Endpoints.AdoptionRequestEndpoints;
using static sapica_backend.Endpoints.AdoptionRequestEndpoints.AdoptionRequestEndpoints;
using Stripe;

namespace sapica_tests
{
    public class CreateAdoptionRequestTest
    {
        private readonly AdoptionRequestEndpoints _adoptionEndpoints;
        private readonly ApplicationDbContext _dbContext;
        private readonly AdoptionRequestValidator _validator;

        public CreateAdoptionRequestTest()
        {
            var dbName = "TestDb_AddAdoptionRequest_" + Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _validator = new AdoptionRequestValidator(_dbContext);

            _dbContext.Country.Add(new Country { Id = 1, Name = "Test Country", Latitude = 1, Longitude = 1 });
            _dbContext.City.Add(new City
            {
                Id = 1,
                Name = "Test City",
                PostalCode = "00000",
                Latitude = 1,
                Longitude = 1,
                CountryId = 1
            });
           
            _dbContext.Animal.Add(new Animal { Id=1, Age="2", AnimalType="Cat", Color="white", Gender="F", HasPassport=true, Name="Lexy", ParasiteFree=false, Size="small", Sterilized=true,Vaccinated=true, Weight=5 });
            _dbContext.AnimalImage.Add(new AnimalImage { AnimalId = 1, Id = 1, Image = Convert.FromBase64String("dgcgdcgcgdcv") });
            _dbContext.AdoptionPost.Add(new AdoptionPost { Id = 1, AnimalId = 1, CityId = 1, DateOfAdoption = DateTime.Now, DateOfCreation = new DateOnly(2025, 1, 21), IsAdopted = false, ShortDescription = "gudgdggd", Urgent = true, Username = "user" });
            _dbContext.SaveChanges();

            _adoptionEndpoints = new AdoptionRequestEndpoints(_dbContext);
        }
        [Fact]
        public async Task Should_Create_AdoptionRequest_When_Valid_Request()
        {
            var request = new AdoptionRequestCreateRequest
            {
                Date = DateTime.Now,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "061234567",
                Reason = "I love animals",
                LivingSpace = "House",
                Backyard = "No",
                Age = 25,
                FamilyMembers = 2,
                AnyKids = "No",
                AnyAnimalsBefore = "Yes",
                TimeCommitment = 5,
                PreferredCharacteristic = "Friendly",
                AdoptionPostId = 1,
                CityId = 1
               
            };

            var result = await _adoptionEndpoints.AddAdoptionRequest(request, CancellationToken.None);

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            var adoptionResponse = Assert.IsType<AdoptionRequest>(createdAtResult.Value);
            Assert.NotNull(adoptionResponse);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_AdoptionPost_Does_Not_Exist()
        {
            var request = new AdoptionRequestCreateRequest
            {
                Date = DateTime.Now,
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@example.com",
                PhoneNumber = "+38761234568",
                Reason = "I want to adopt",
                LivingSpace = "House",
                Backyard = "Yes",
                Age = 30,
                FamilyMembers = 3,
                AnyKids = "Yes",
                AnyAnimalsBefore = "No",
                Experience = "No experience",
                TimeCommitment = 10,
                PreferredCharacteristic = "Calm",
                AdoptionPostId = 999, // Invalid ID
                CityId = 1
            };

            var result = await _adoptionEndpoints.AddAdoptionRequest(request, CancellationToken.None);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Provided AdoptionPostId or CityId does not exist", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_CityId_Does_Not_Exist()
        {
            var request = new AdoptionRequestCreateRequest
            {
                Date = DateTime.Now,
                FirstName = "Charlie",
                LastName = "Brown",
                Email = "charlie@example.com",
                PhoneNumber = "+38761234572",
                Reason = "I have a big backyard",
                LivingSpace = "House",
                Backyard = "Yes",
                Age = 40,
                FamilyMembers = 4,
                AnyKids = "No",
                AnyAnimalsBefore = "Yes",
                Experience = "Had a cat before",
                TimeCommitment = 6,
                PreferredCharacteristic = "Playful",
                AdoptionPostId = 1,
                CityId = 999 // Invalid CityId
            };

            var result = await _adoptionEndpoints.AddAdoptionRequest(request, CancellationToken.None);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Provided AdoptionPostId or CityId does not exist", badRequestResult.Value.ToString());
        }

        [Fact]
        public void Should_Have_Error_When_FirstName_Is_Too_Short()
        {
            var request = new AdoptionRequestCreateRequest { FirstName = "", LastName = "ValidLast", Email = "valid@email.com", PhoneNumber = "+38761234567", Reason = "Valid reason", LivingSpace = "Flat", Backyard = "No", Age = 25, FamilyMembers = 2, AnyKids = "No", AnyAnimalsBefore = "No", TimeCommitment = 3, PreferredCharacteristic = "Friendly", AdoptionPostId = 1, CityId = 1 };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.FirstName);
        }
        [Fact]
        public void Should_Have_Error_When_TimeCommitment_Is_Too_High()
        {
            var request = new AdoptionRequestCreateRequest{ FirstName = "ValidFirst", LastName = "ValidLast", Email = "valid@email.com", PhoneNumber = "+38761234567", Reason = "Valid reason", LivingSpace = "Flat", Backyard = "No", Age = 25, FamilyMembers = 2, AnyKids = "No", AnyAnimalsBefore = "No", TimeCommitment = 27, PreferredCharacteristic = "Friendly", AdoptionPostId = 1, CityId = 1 };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.TimeCommitment);
        }

        [Fact]
        public void Should_Have_Error_When_Age_Is_Less_Than_18()
        {
            var request = new AdoptionRequestCreateRequest { FirstName = "ValidFirst", LastName = "ValidLast", Email = "valid@email.com", PhoneNumber = "+38761234567", Reason = "Valid reason", LivingSpace = "Flat", Backyard = "No", Age = 17, FamilyMembers = 2, AnyKids = "No", AnyAnimalsBefore = "No", TimeCommitment = 3, PreferredCharacteristic = "Friendly", AdoptionPostId = 1, CityId = 1  };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Age);
        }
        [Fact]
        public void Should_Have_Error_When_TimeCommitment_Is_Zero()
        {
            var request = new AdoptionRequestCreateRequest { FirstName = "ValidFirst", LastName = "ValidLast", Email = "valid@email.com", PhoneNumber = "+38761234567", Reason = "Valid reason", LivingSpace = "Flat", Backyard = "No", Age = 19, FamilyMembers = 2, AnyKids = "No", AnyAnimalsBefore = "No", TimeCommitment = 0, PreferredCharacteristic = "Friendly", AdoptionPostId = 1, CityId = 1 };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.TimeCommitment);
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            var request = new AdoptionRequestCreateRequest { FirstName = "ValidFirst", LastName = "ValidLast", Email = "email-invalid", PhoneNumber = "+38761234567", Reason = "Valid reason", LivingSpace = "Flat", Backyard = "No", Age = 19, FamilyMembers = 2, AnyKids = "No", AnyAnimalsBefore = "No", TimeCommitment = 3, PreferredCharacteristic = "Friendly", AdoptionPostId = 1, CityId = 1 };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }
    }
    }


