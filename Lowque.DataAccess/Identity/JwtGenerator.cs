using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Lowque.DataAccess.Identity
{
    public class JwtGenerator : IJwtGenerator
    {
        private readonly IConfiguration appConfiguration;

        public JwtGenerator(IConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
        }

        public string Generate(string username, string[] roles)
        {
            var jwtHeader = GenerateJwtHeader();
            var jwtPayload = GenerateJwtPayload(username, roles);
            var jwtData = new JwtSecurityToken(jwtHeader, jwtPayload);
            
            var jwt = new JwtSecurityTokenHandler().WriteToken(jwtData);           
            return jwt;
        }

        private JwtHeader GenerateJwtHeader()
        {
            var signingSecret = Encoding.UTF8.GetBytes(appConfiguration["Jwt:Key"]);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(signingSecret), SecurityAlgorithms.HmacSha256);
            
            var jwtHeader = new JwtHeader(signingCredentials);
            return jwtHeader;
        }

        private JwtPayload GenerateJwtPayload(string username, string[] roles)
        {
            var issuer = appConfiguration["Jwt:Issuer"];
            var audience = issuer;
            var notBefore = DateTime.Now;
            var expires = notBefore.AddMinutes(60 * 24);
            var issuedAt = notBefore;

            var additionalClaims = new List<Claim>();
            additionalClaims.Add(new Claim(JwtRegisteredClaimNames.Sub, username));
            additionalClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            additionalClaims.AddRange(roles.Select(role => new Claim("roles", role)));

            var jwtPayload = new JwtPayload(issuer, audience, additionalClaims, notBefore, expires, issuedAt);
            return jwtPayload;
        }
    }
}
