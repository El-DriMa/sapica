using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sapica_backend.Data;
using sapica_backend.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace sapica_backend.Endpoints.AuthEndpoints
{
    public class LogoutEndpoint(ApplicationDbContext db) : ControllerBase
    {
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
        {
            string? authHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return BadRequest(new LogoutResponse
                {
                    IsSuccess = false,
                    Message = "Authorization token is missing or invalid."
                });
            }

            string jwtToken = authHeader.Substring("Bearer ".Length).Trim();

            var tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwt;
            try
            {
                jwt = tokenHandler.ReadJwtToken(jwtToken);
            }
            catch
            {
                return BadRequest(new LogoutResponse
                {
                    IsSuccess = false,
                    Message = "Invalid JWT token."
                });
            }

            var userIdClaim = jwt?.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null)
            {
                return BadRequest(new LogoutResponse
                {
                    IsSuccess = false,
                    Message = "User ID not found in token."
                });
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return BadRequest(new LogoutResponse
                {
                    IsSuccess = false,
                    Message = "Invalid User ID in token."
                });
            }

            var isRevoked = await AuthService.RevokeAllRefreshTokensForUserAsync(db, userId, cancellationToken);

            return Ok(new LogoutResponse
            {
                IsSuccess = isRevoked,
                Message = isRevoked ? "Logout successful." : "Failed to revoke tokens. User may already be logged out."
            });
        }

        public class LogoutResponse
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
        }
    }
}
