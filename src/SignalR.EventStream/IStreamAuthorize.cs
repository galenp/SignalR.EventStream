using System.Security.Principal;

namespace SignalR
{
    public interface IStreamAuthorize
    {
        bool Authorized(string clientId, IPrincipal user, string group);
    }
}