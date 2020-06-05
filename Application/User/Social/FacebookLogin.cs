using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Application.Interfaces.Social;
using Data.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.User.Social
{
    public class FacebookLogin
    {
        public class Command : IRequest<User>
        {
            public string AccessToken { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.AccessToken).NotEmpty();
            }
        }


        public class Handler : IRequestHandler<Command, User>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly IFacebookAccessor _facebookAccessor;
            private readonly IJwtGenerator _jwtGenerator;
            private readonly IUserActivitiesApp _userActivitiesApp;
            public Handler(UserManager<AppUser> userManager, IFacebookAccessor facebookAccessor,
            IJwtGenerator jwtGenerator, IUserActivitiesApp userActivitiesApp)
            {
                _userActivitiesApp = userActivitiesApp;
                _jwtGenerator = jwtGenerator;
                _facebookAccessor = facebookAccessor;
                _userManager = userManager;
            }

            public async Task<User> Handle(Command request, CancellationToken cancellationToken)
            {
                var userInfo = await _facebookAccessor.FacebookLogin(request.AccessToken);

                if (userInfo == null)
                    throw new RestException(HttpStatusCode.BadGateway, new { User = "Problem validating fb token" });

                var user = await _userManager.FindByEmailAsync(userInfo.Email);

                var token = "";
                if (user == null)
                {
                    user = new AppUser
                    {
                        DisplayName = userInfo.Name,
                        Id = userInfo.Id,
                        Email = userInfo.Email,
                        UserName = "fb_" + userInfo.Id,
                        RefreshToken = _jwtGenerator.GenerateRefreshToken(),
                        RefreshTokenExpiry = DateTime.UtcNow.AddDays(30)
                    };

                    //try create local user
                    token = _jwtGenerator.CreateToken(user);
                    var userCreated = await _userActivitiesApp.CreateUser(user.DisplayName, token);
                    if (!userCreated)
                        throw new RestException(HttpStatusCode.BadRequest, new { User = "Problem creating local fb user" });

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                        throw new RestException(HttpStatusCode.BadRequest, new { User = "Problem creating fb user" });
                }

                if (token == "")
                    token = _jwtGenerator.CreateToken(user);

                return new User
                {
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    Username = user.UserName,
                    Token = token,
                    RefreshToken = user.RefreshToken
                };

            }
        }
    }
}