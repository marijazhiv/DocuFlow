using DocuFlowAPI.Models;

namespace DocuFlowAPI.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }

}
