using Microsoft.IdentityModel.Tokens;
using Quizapp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Quizapp.Services
{
    public class TokenService : ITokenService
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public TokenService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public int GetUserId()
        {
            var identity = _httpContextAccessor?.HttpContext?.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                string id = identity.Claims.FirstOrDefault(o => o.Type == "UserId")?.Value;
                if (id != "" && id != null)
                {
                    return Convert.ToInt32(id);
                }
            }

            return 0;
        }

        public string CreateToken(User userDetails)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Role , userDetails.Role),
                new Claim(ClaimTypes.Name, userDetails.Name),
                new Claim("UserId", userDetails.UserId.ToString())
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("Jwt:Key").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                _configuration.GetSection("Jwt:Issuer").Value,
                _configuration.GetSection("Jwt:Audience").Value,
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

    }
}
