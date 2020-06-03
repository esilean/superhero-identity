using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using MediatR;
using System.Text.Json;
using System.Net;
using Application.Errors;

namespace Infrastructure.User
{
    public class UserActivitiesApp : IUserActivitiesApp
    {
        private readonly IHttpClientFactory _clientFactory;

        public UserActivitiesApp(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<bool> CreateUser(string displayName, string token)
        {

            var user = JsonSerializer.Serialize(new UserActivity { DisplayName = displayName });
            var content = new StringContent(user, Encoding.UTF8, "application/json");


            var client = _clientFactory.CreateClient("activities");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var response = await client.PostAsync("api/user/local", content);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        throw new RestException(response.StatusCode);
                    case HttpStatusCode.BadRequest:
                        throw new RestException(response.StatusCode, new { User = "Something happened" });
                    default:
                        throw new RestException(response.StatusCode, new { User = "Server error" });
                }
            }

            return response.IsSuccessStatusCode;
        }
    }
}