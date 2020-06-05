using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.User;
using Application.User.Social;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        public UserController(IMediator mediator, IConfiguration configuration)
        {
            _configuration = configuration;
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] Login.Query query)
        {
            return await _mediator.Send(query);
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] Register.Command command)
        {
            return await _mediator.Send(command);
        }

        [HttpPost("facebook")]
        public async Task<ActionResult<User>> FacebookLogin(FacebookLogin.Command command)
        {
            return await _mediator.Send(command);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<User>> RefreshToken(RefreshToken.Query query)
        {
            var principal = GetPrincipalFromExpiredToken(query.Token);
            query.Username = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            return await _mediator.Send(query);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["TokenKey"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid Token");

            return principal;
        }
    }
}