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
using sapica_backend.Endpoints.UserEndpoints;
using sapica_backend.Services;
using FluentValidation.TestHelper;

namespace sapica_tests
{
    public class UserRegistrationTest
    {
        private readonly UserEndpoints _userEndpoints;
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<EmailService> _emailServiceMock;
        private readonly UserCreateValidator _validator;

        public UserRegistrationTest()
        {
            // Use a unique database name for each test instance
            var dbName = "TestDb_AddUser_" + Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _validator = new UserCreateValidator(_dbContext);

            // Seed required Country and City records (CityId = 1 is used in our requests)
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
            _dbContext.SaveChanges();

            // Create a dummy HttpContextAccessor (not used by AddUser but required by constructor)
            var httpContextAccessor = new HttpContextAccessor();

            _emailServiceMock = new Mock<EmailService>();
            _emailServiceMock
                .Setup(es => es.SendActivationEmail(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _userEndpoints = new UserEndpoints(_dbContext, httpContextAccessor, _emailServiceMock.Object);
        }

        [Fact]
        public async Task Should_Create_User_When_Valid_Request()
        {
            var request = new UserCreateRequest
            {
                FirstName = "John",
                LastName = "Doe",
                YearBorn = 2000,
                Username = "johndoe",
                Password = "Password123",
                Email = "john@example.com",
                PhoneNumber = "+38761234567",
                ImageUrl = "http://example.com/avatar.jpg",
                CityId = 1
            };

            ActionResult result = await _userEndpoints.AddUser(request, CancellationToken.None);

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            var userResponse = Assert.IsType<UserReadResponse>(createdAtResult.Value);
            Assert.NotNull(userResponse);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Username_Already_Taken()
        {
            var request1 = new UserCreateRequest
            {
                FirstName = "Alice",
                LastName = "Smith",
                YearBorn = 1990,
                Username = "alice",
                Password = "Password123",
                Email = "alice@example.com",
                PhoneNumber = "+38761234568",
                ImageUrl = "http://example.com/alice.jpg",
                CityId = 1
            };

            var result1 = await _userEndpoints.AddUser(request1, CancellationToken.None);
            Assert.IsType<CreatedAtActionResult>(result1);

            var request2 = new UserCreateRequest
            {
                FirstName = "Alice2",
                LastName = "Smith2",
                YearBorn = 1991,
                Username = "alice", // duplicate username
                Password = "Password123",
                Email = "alice2@example.com",
                PhoneNumber = "+38761234569",
                ImageUrl = "http://example.com/alice2.jpg",
                CityId = 1
            };

            ActionResult result2 = await _userEndpoints.AddUser(request2, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result2);
            Assert.Equal("Username is already taken.", badRequestResult.Value);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Email_Already_In_Use()
        {
            var request1 = new UserCreateRequest
            {
                FirstName = "Bob",
                LastName = "Jones",
                YearBorn = 1985,
                Username = "bobjones",
                Password = "Password123",
                Email = "bob@example.com",
                PhoneNumber = "+38761234570",
                ImageUrl = "http://example.com/bob.jpg",
                CityId = 1
            };

            var result1 = await _userEndpoints.AddUser(request1, CancellationToken.None);
            Assert.IsType<CreatedAtActionResult>(result1);

            var request2 = new UserCreateRequest
            {
                FirstName = "Bobby",
                LastName = "Jones",
                YearBorn = 1986,
                Username = "bobbyj",
                Password = "Password123",
                Email = "bob@example.com", // duplicate email
                PhoneNumber = "+38761234571",
                ImageUrl = "http://example.com/bobby.jpg",
                CityId = 1
            };

            ActionResult result2 = await _userEndpoints.AddUser(request2, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result2);
            Assert.Equal("Email is already in use.", badRequestResult.Value);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_CityId_Does_Not_Exist()
        {
            var request = new UserCreateRequest
            {
                FirstName = "Charlie",
                LastName = "Brown",
                YearBorn = 1995,
                Username = "charlieb",
                Password = "Password123",
                Email = "charlie@example.com",
                PhoneNumber = "+38761234572",
                ImageUrl = "http://example.com/charlie.jpg",
                CityId = 999 // Non-existent CityId
            };

            ActionResult result = await _userEndpoints.AddUser(request, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Provided CityId does not exist", badRequestResult.Value.ToString());
        }

        [Fact]
        public void Should_Have_Error_When_FirstName_Is_Too_Short()
        {
            var request = new UserCreateRequest { FirstName = "A", LastName = "ValidLast", YearBorn = 2000, Username = "ValidUser", Password = "Valid123!", Email = "valid@email.com", PhoneNumber = "+38761234567", CityId = 1 };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.FirstName);
        }

        [Fact]
        public void Should_Have_Error_When_FirstName_Is_Too_Long()
        {
            var request = new UserCreateRequest { FirstName = new string('A', 21), LastName = "ValidLast", YearBorn = 2000, Username = "ValidUser", Password = "Valid123!", Email = "valid@email.com", PhoneNumber = "+38761234567", CityId = 1 };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.FirstName);
        }

        [Fact]
        public void Should_Have_Error_When_LastName_Is_Too_Short()
        {
            var request = new UserCreateRequest { FirstName = "ValidFirst", LastName = "A", YearBorn = 2000, Username = "ValidUser", Password = "Valid123!", Email = "valid@email.com", PhoneNumber = "+38761234567", CityId = 1 };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.LastName);
        }

        [Fact]
        public void Should_Have_Error_When_YearBorn_Is_Invalid()
        {
            var request = new UserCreateRequest { FirstName = "ValidFirst", LastName = "ValidLast", YearBorn = 2022, Username = "ValidUser", Password = "Valid123!", Email = "valid@email.com", PhoneNumber = "+38761234567", CityId = 1 };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.YearBorn);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Invalid()
        {
            var request = new UserCreateRequest { FirstName = "ValidFirst", LastName = "ValidLast", YearBorn = 2000, Username = "ValidUser", Password = "weakpass", Email = "valid@email.com", PhoneNumber = "+38761234567", CityId = 1 };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Have_Error_When_PhoneNumber_Is_Invalid()
        {
            var request = new UserCreateRequest { FirstName = "ValidFirst", LastName = "ValidLast", YearBorn = 2000, Username = "ValidUser", Password = "Valid123!", Email = "valid@email.com", PhoneNumber = "123456", CityId = 1 };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
        }
    }
}
