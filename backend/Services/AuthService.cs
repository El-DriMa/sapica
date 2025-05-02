using sapica_backend.Data;
using sapica_backend.Data.Models;
using sapica_backend.Data.Models.Auth;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Cryptography;
using System.Threading;

namespace sapica_backend.Services
{
    public class AuthService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        public static string GenerateJwtToken(UserAccount user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("1f0555a6c0e8f624c0237b1cc6bec0dd1ceab3b17354314dcfecadea9324a2f1");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("Role", user.isAdmin ? "Admin" : user.isUser ? "User" : "Shelter")
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = "https://localhost:7291/",
                Issuer = "https://localhost:7291/"
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string GenerateRefreshToken(int userAccountId, ApplicationDbContext db)
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var refreshToken = Convert.ToBase64String(randomBytes);

            var existingToken = db.RefreshToken
                .FirstOrDefault(rt => rt.UserAccountId == userAccountId);

            if (existingToken != null)
            {
                existingToken.Token = refreshToken;
                existingToken.ExpiryDate = DateTime.UtcNow.AddDays(7);
                existingToken.IsRevoked = false;
            }
            else
            {
                var tokenEntry = new RefreshToken
                {
                    Token = refreshToken,
                    UserAccountId = userAccountId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false
                };
                db.RefreshToken.Add(tokenEntry);
            }

            db.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<UserAccount?> ValidateRefreshTokenAsync(string token)
        {
            var storedToken = await db.RefreshToken
                .Include(rt => rt.UserAccount)
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow);

            if (storedToken == null)
                return null;

            return storedToken.UserAccount;
        }

        public static async Task RevokeRefreshTokenAsync(ApplicationDbContext db, string token)
        {
            var storedToken = await db.RefreshToken.FirstOrDefaultAsync(rt => rt.Token == token);
            
            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
                await db.SaveChangesAsync();
            }
        }

        public static async Task<bool> RevokeAllRefreshTokensForUserAsync(ApplicationDbContext db, int userId, CancellationToken cancellationToken = default)
        {
            var tokens = await db.RefreshToken
                .Where(rt => rt.UserAccountId == userId && !rt.IsRevoked)
                .ToListAsync(cancellationToken);

            if (!tokens.Any())
            {
                return false;
            }

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }


        public static async Task<string> GenerateAccessTokenFromRefreshTokenAsync(ApplicationDbContext db, string refreshToken)
        {
            var storedRefreshToken = await db.RefreshToken
                .Include(rt => rt.UserAccount)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

            if (storedRefreshToken == null || storedRefreshToken.ExpiryDate <= DateTime.UtcNow)
            {
                throw new SecurityTokenException("Invalid or expired refresh token.");
            }

            var user = storedRefreshToken.UserAccount;
            if (user == null)
            {
                throw new SecurityTokenException("Associated user not found.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("1f0555a6c0e8f624c0237b1cc6bec0dd1ceab3b17354314dcfecadea9324a2f1");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("Role", user.isAdmin ? "Admin" : user.isUser ? "User" : "Shelter")
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = "https://localhost:7291/",
                Issuer = "https://localhost:7291/",
                IssuedAt = DateTime.UtcNow
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static async Task<object?> GetCurrentUserAsync(
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor,
            CancellationToken cancellationToken = default)
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                Console.WriteLine("HttpContext User is null.");
                return null;
            }

            var usernameClaim = user.FindFirst(ClaimTypes.Name);
            var roleClaim = user.FindFirst("Role");

            if (usernameClaim == null || roleClaim == null)
            {
                Console.WriteLine("Required claims are missing. UsernameClaim: {0}, RoleClaim: {1}", usernameClaim, roleClaim);
                return null;
            }

            var username = usernameClaim.Value;
            var role = roleClaim.Value;

            Console.WriteLine("Extracted Username: {0}, Role: {1}", username, role);

            object? result = null;
            switch (role)
            {
                case "User":
                    result = await db.User.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
                    break;
                case "Shelter":
                    result = await db.Shelter.FirstOrDefaultAsync(s => s.Username == username, cancellationToken);
                    break;
                case "Admin":
                    result = await db.Admin.FirstOrDefaultAsync(a => a.Username == username, cancellationToken);
                    break;
                default:
                    Console.WriteLine("Unknown role: {0}", role);
                    break;
            }

            Console.WriteLine("Database query result for role {0}: {1}", role, result);

            return result;
        }

    }
}
