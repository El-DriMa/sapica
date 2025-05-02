using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using sapica_backend.Data.Models;
using sapica_backend.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Services;
using sapica_backend.Migrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace sapica_backend.Endpoints.AuthEndpoints
{
    [Route("login")]
    [ApiController]
    public class LoginEndpoint(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        [HttpPost("")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var isEmail = request.UsernameOrEmail.Contains("@");

            var userAccount = await db.UserAccount
                .FirstOrDefaultAsync(x =>
                    (isEmail ? x.Email == request.UsernameOrEmail : x.Username == request.UsernameOrEmail),
                    cancellationToken);

            if (userAccount == null)
            {
                return Unauthorized(new { message = "Invalid username/email or password." });
            }

            var isPasswordValid = BCrypt.Net.BCrypt.EnhancedVerify(request.Password, userAccount.Password);

            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Invalid username/email or password" });
            }

            if (userAccount.isUser == true)
            {
                var userCheck = db.User.Where(u => u.Username == userAccount.Username).First();
                if (userCheck.IsActivated == false)
                {
                    return Unauthorized(new { message = "User profile not activated" });
                }
            }

            if (userAccount.isShelter == true)
            {
                var shelterCheck = db.Shelter.Where(s => s.Username == userAccount.Username).First();
                if (shelterCheck.IsActivated == false)
                {
                    return Unauthorized(new { message = "Shelter profile not activated" });
                }
            }

            var accessToken = AuthService.GenerateJwtToken(userAccount);
            var refreshToken = AuthService.GenerateRefreshToken(userAccount.Id, db);

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserAccount = new
                {
                    userAccount.Id,
                    userAccount.Username,
                    userAccount.Email
                }
            });
        }

        [Authorize]
        [HttpGet("GetCurrentUser")]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            var currentUser = await AuthService.GetCurrentUserAsync(db, httpContextAccessor, cancellationToken);

            if (currentUser == null)
            {
                return NotFound(new { Message = "No logged-in user found or invalid role." });
            }

            var CurrentUser = (UserAccount)currentUser;
            CurrentUser.City = db.City.Where(x => x.Id == CurrentUser.CityId).First();

            return Ok(CurrentUser);
        }
    }

    public class LoginRequest
    {
        public required string UsernameOrEmail { get; set; }
        public required string Password { get; set; }
    }
}
