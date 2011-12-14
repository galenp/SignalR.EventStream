using System.Security.Principal;

namespace SignalR
{
    public interface IStreamAuthorize
    {
        bool Authorized(ref string clientId, IPrincipal user, string group);
    }
}