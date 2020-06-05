using System.Threading.Tasks;
using Application.User.Social;

namespace Application.Interfaces.Social
{
    public interface IFacebookAccessor
    {
        Task<FacebookUserInfo> FacebookLogin(string accessToken);
    }
}