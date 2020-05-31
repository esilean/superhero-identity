using System.Threading.Tasks;
using Application.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator mediator;
        public UserController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] Login.Query query)
        {
            return await this.mediator.Send(query);
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] Register.Command command)
        {
            return await this.mediator.Send(command);
        }


        [Authorize]
        [HttpGet("current")]
        public async Task<ActionResult<User>> CurrentUser()
        {
            return await this.mediator.Send(new CurrentUser.Query());
        }
    }
}