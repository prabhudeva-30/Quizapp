using Quizapp.Models;

namespace Quizapp.Services
{
    public interface ITokenService
    {
        int GetUserId();
        string CreateToken(User userDetails);

    }
}
