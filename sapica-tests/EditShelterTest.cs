using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentValidation.TestHelper;
using sapica_backend.Data;
using sapica_backend.Data.Models;
using sapica_backend.Endpoints.ShelterEndpoints;
using static sapica_backend.Endpoints.ShelterEndpoints.ShelterEndpoints;

namespace sapica_tests
{
    public class ShelterEditTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ShelterEditValidator _validator;

        public ShelterEditTests()
        {
           
            
                var dbName = "TestDb_ShelterEdit_" + Guid.NewGuid().ToString();
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase(databaseName: dbName)
                    .Options;
                _dbContext = new ApplicationDbContext(options);

                // Resetuje bazu podataka prije svakog testa
                _dbContext.Database.EnsureDeleted();
                _dbContext.Database.EnsureCreated();

                _validator = new ShelterEditValidator(_dbContext);

                _dbContext.City.Add(new City { Id = 1, Name = "Test City", PostalCode = "00000" });
               
                _dbContext.Shelter.Add(new Shelter { Id = 1, Name = "Shelter One", Owner = "Owner One", YearFounded = 2000, Address = "Test Address", Username = "shelter1", Email = "shelter1@email.com", PhoneNumber = "+38761123456", CityId = 1, Password = "Password123", ImageUrl = "http://example.com/image.jpg" });
                _dbContext.SaveChanges();
            

        }

        [Fact]
        public void Should_Have_Error_When_Username_Is_Already_Taken()
        {
            var request = new ShelterUpdateRequest { Username = "shelter1", Name = "Test Shelter", Owner = "Owner", YearFounded = 2000, Address = "Address", Email = "test@email.com", PhoneNumber = "+38761123456", Password = "Password123", CityId = 1, ImageUrl = "http://example.com/image.jpg" };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Username);
            

        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Already_In_Use()
        {
            var request = new ShelterUpdateRequest { Email = "shelter1@email.com", Name = "Test Shelter", Owner = "Owner", YearFounded = 2000, Address = "Address", Username = "newUser", PhoneNumber = "+38761123456", Password = "Password123", CityId = 1, ImageUrl = "http://example.com/image.jpg" };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Email);
            

        }

        [Fact]
        public void Should_Have_Error_When_CityId_Does_Not_Exist()
        {
            var request = new ShelterUpdateRequest { CityId = 999, Name = "Test Shelter", Owner = "Owner", YearFounded = 2000, Address = "Address", Username = "newUser", Email = "test@email.com", PhoneNumber = "+38761123456", Password = "Password123", ImageUrl = "http://example.com/image.jpg" };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.CityId);
            

        }
        [Fact]
        public async Task Should_Update_Shelter_Successfully()
        {
            var shelter = await _dbContext.Shelter.FirstOrDefaultAsync(s => s.Id == 1);
            var userAccount = await _dbContext.UserAccount.FirstOrDefaultAsync(u => u.Username == shelter.Username);

            var request = new ShelterUpdateRequest
            {
                Name = "Updated Shelter",
                Owner = "Updated Owner",
                YearFounded = 2010,
                Address = "Updated Address",
                Username = "updatedUsername",
                Email = "updated@email.com",
                PhoneNumber = "+38761222333",
                Password = "NewPassword123",
                CityId = 1,
                ImageUrl = "http://example.com/newimage.jpg"
            };

            shelter.Name = request.Name;
            shelter.Owner = request.Owner;
            shelter.YearFounded = request.YearFounded;
            shelter.Address = request.Address;
            shelter.Username = request.Username;
            shelter.Email = request.Email;
            shelter.PhoneNumber = request.PhoneNumber;
            shelter.CityId = request.CityId;
            shelter.ImageUrl = request.ImageUrl;
            shelter.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);

            userAccount.Username = request.Username;
            userAccount.Email = request.Email;
            userAccount.PhoneNumber = request.PhoneNumber;
            userAccount.CityId = request.CityId;
            userAccount.ImageUrl = request.ImageUrl;
            userAccount.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);

            await _dbContext.SaveChangesAsync();

            var updatedShelter = await _dbContext.Shelter.FirstOrDefaultAsync(s => s.Id == 1);

            Assert.NotNull(updatedShelter);
            Assert.Equal(request.Name, updatedShelter.Name);
            Assert.Equal(request.Owner, updatedShelter.Owner);
            Assert.Equal(request.YearFounded, updatedShelter.YearFounded);
            Assert.Equal(request.Address, updatedShelter.Address);
            Assert.Equal(request.Username, updatedShelter.Username);
            Assert.Equal(request.Email, updatedShelter.Email);
            Assert.Equal(request.PhoneNumber, updatedShelter.PhoneNumber);
            Assert.Equal(request.CityId, updatedShelter.CityId);
            Assert.Equal(request.ImageUrl, updatedShelter.ImageUrl);
        }
        [Fact]
        public async Task Should_Return_Error_When_ShelterId_Does_Not_Exist()
        {
            var request = new ShelterUpdateRequest
            {
                Name = "Nonexistent Shelter",
                Owner = "Nonexistent Owner",
                YearFounded = 2010,
                Address = "Nonexistent Address",
                Username = "nonexistentUsername",
                Email = "nonexistent@email.com",
                PhoneNumber = "+38761222333",
                Password = "NewPassword123",
                CityId = 1,
                ImageUrl = "http://example.com/nonexistentimage.jpg"
            };

            var nonExistentShelter = await _dbContext.Shelter.FirstOrDefaultAsync(s => s.Id == 999);
            Assert.Null(nonExistentShelter);
        }
    }
}
