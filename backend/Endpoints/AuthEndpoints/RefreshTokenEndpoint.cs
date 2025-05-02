using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using sapica_backend.Data;
using sapica_backend.Services;


namespace sapica_backend.Endpoints.AuthEndpoints
{
    [Microsoft.AspNetCore.Mvc.Route("refresh")]
    [ApiController]
    public class RefreshTokenEndpoint(ApplicationDbContext db) : ControllerBase
    {
        [HttpPost("")]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var newAccessToken = await AuthService.GenerateAccessTokenFromRefreshTokenAsync(db, request.RefreshToken);

                return Ok(new { 
                    AccessToken = newAccessToken,
                    RefreshToken = request.RefreshToken
                });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }

    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
