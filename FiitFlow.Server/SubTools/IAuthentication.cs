using FiitFlow.Server.SubTools.SubToolsUnits;

namespace FiitFlow.Server.SubTools
{
    public interface IAuthentication
    {
        Task<AuthResponse<int>> FindAuthIdByLoginForm(string firstName, string lastName, string groupFull);
    }
}
