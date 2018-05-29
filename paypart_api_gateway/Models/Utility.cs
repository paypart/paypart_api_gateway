using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace paypart_api_gateway.Models
{
    public class Utility
    {
        IOptions<Settings> _settings;
        //private readonly RequestDelegate _next;

        public Utility(IOptions<Settings> settings)
        {
            _settings = settings;
            //this._next = next;
        }

        public dynamic getJWT(string name)
        {
            var now = DateTime.UtcNow;

            var claims = new Claim[]
            {
                   new Claim(JwtRegisteredClaimNames.Sub, name),
                   new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                   new Claim(JwtRegisteredClaimNames.Iat, now.ToUniversalTime().ToString(), ClaimValueTypes.Integer64)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_settings.Value.Secret));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = _settings.Value.Iss,
                ValidateAudience = true,
                ValidAudience = _settings.Value.Aud,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,

            };

            var jwt = new JwtSecurityToken(
                issuer: _settings.Value.Iss,
                audience: _settings.Value.Aud,
                claims: claims,
                notBefore: now,
                expires: now.Add(TimeSpan.FromMinutes(_settings.Value.timespan)),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var responseJson = new
            {
                access_token = encodedJwt,
                expires_in = (int)TimeSpan.FromMinutes(_settings.Value.timespan).TotalSeconds
            };

            return responseJson;
        }
        public bool appendToken(HttpContext ctx, User user)
        {
            bool isappended = false;
            var jwt = getJWT(user.email);
            ctx.Response.Headers.Add("Token", $"Bearer {jwt.access_token}");
            isappended = true;

            return isappended;
        }
    }
}
