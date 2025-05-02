using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using sapica_backend.Data;
using sapica_backend.Data.Models;
using sapica_backend.Endpoints.AuthEndpoints;

namespace sapica_tests
{
    public class UserLoginTest
    {
        private readonly LoginEndpoint _loginEndpoint;
        private readonly ApplicationDbContext _dbContext;

        public UserLoginTest()
        {
            var dbName = "TestDb_LoginUser_" + Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            _dbContext = new ApplicationDbContext(options);

            // Ensure database is created
            _dbContext.Database.EnsureCreated();

            var httpContextAccessor = new HttpContextAccessor();
            _loginEndpoint = new LoginEndpoint(_dbContext, httpContextAccessor);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
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

            _dbContext.UserAccount.Add(new UserAccount
            {
                Id = 1,
                Username = "testuser",
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword("Password123"),
                Email = "test@example.com",
                PhoneNumber = "060000000",
                ImageUrl = "http://example.com/avatar.jpg",
                CityId = 1,
                isAdmin = true,
                isShelter = false,
                isUser = false
            });

            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task Should_Login_Successfully_With_Valid_Credentials()
        {
            var users = await _dbContext.UserAccount.ToListAsync();
            Console.WriteLine($"Users in DB: {users.Count}");
            foreach (var user in users)
            {
                Console.WriteLine($"User: {user.Username}, PasswordHash: {user.Password}");
            }

            var request = new LoginRequest
            {
                UsernameOrEmail = "testuser",
                Password = "Password123"
            };

            IActionResult result = await _loginEndpoint.Login(request, CancellationToken.None);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Should_Return_Unauthorized_When_Invalid_Password()
        {
            var request = new LoginRequest
            {
                UsernameOrEmail = "testuser",
                Password = "wrongpassword"
            };

            IActionResult result = await _loginEndpoint.Login(request, CancellationToken.None);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Should_Return_Unauthorized_When_User_Does_Not_Exist()
        {
            var request = new LoginRequest
            {
                UsernameOrEmail = "nonexistent",
                Password = "somepassword"
            };

            IActionResult result = await _loginEndpoint.Login(request, CancellationToken.None);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
