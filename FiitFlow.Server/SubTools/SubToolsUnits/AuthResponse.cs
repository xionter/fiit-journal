namespace FiitFlow.Server.SubTools.SubToolsUnits
{
    public record AuthResponse<IData>(bool Accepted, IData Data, string exceptionMessage);
}
