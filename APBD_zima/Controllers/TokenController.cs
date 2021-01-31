
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using APBD_zima.Dtos;
using APBD_zima.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace APBD_zima.Controllers
{
    [ApiController]
    [Route("api/tokens")]
    public class TokenController : ControllerBase
    {
        
        public IConfiguration Configuration { get; set; }
        private readonly IStudentsDbService _dbService;
        public TokenController (IStudentsDbService ser, IConfiguration configuration)
        {
            Configuration = configuration;
            _dbService = ser;
        }

        [HttpPost("refresh")]
        public IActionResult Refresh(TokenRefreshRequestDto tokenRefreshRequestDto)
        {
            var token = tokenRefreshRequestDto.token;
            var refreshToken = tokenRefreshRequestDto.refreshToken;

            var principal = GetPrincipalFromExpiredToken(token);
            var index = principal.Identity.Name;
            var savedRefreshToken = _dbService.GetRefreshToken(index);
            if (savedRefreshToken != refreshToken)
                return BadRequest("Wrong refresh token provided");

            var newJwtToken = GenerateToken(principal.Claims);
            var newRefreshToken = Guid.NewGuid();
            _dbService.SaveRefreshToken(index, newRefreshToken.ToString());

            return Ok(new
            {
                newJwtToken,
                newRefreshToken
            });
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, 
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
        private string GenerateToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
              (
                  issuer: "Gakko",
                  audience: "Students",
                  claims: claims,
                  expires: DateTime.Now.AddMinutes(10),
                  signingCredentials: creds
              );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
