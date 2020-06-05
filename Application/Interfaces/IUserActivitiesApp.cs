using System.Threading.Tasks;
using MediatR;

namespace Application.Interfaces
{
    public interface IUserActivitiesApp
    {
        Task<bool> CreateUser(string displayName, string token);
    }
}