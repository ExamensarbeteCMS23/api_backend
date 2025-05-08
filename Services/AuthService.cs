using api_backend.Interfaces;
using api_backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace api_backend.Services
{
    public class AuthService(UserManager<ApplicationUser> userManager, IConfiguration config) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IConfiguration _config = config;

        public async Task<(string Token, DateTime expiration)> AuthenticateAsync(string Email, string Password)
        {
            // Hämtar eposten från databasen
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, Password))
                throw new UnauthorizedAccessException("Fel användare eller lösenord");

            var roles = await _userManager.GetRolesAsync(user);

            // Sätter Claims som kommer in i Token senare
            var authClaims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id),
            };

            // Lägger till roller i Claims
            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["authToken:Key"]));

            var token = new JwtSecurityToken(
                issuer: "testissuer",
                audience: "testaudience",
                expires: DateTime.UtcNow.AddHours(4),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return (new JwtSecurityTokenHandler().WriteToken(token), token.ValidTo);
        }
    }
}
