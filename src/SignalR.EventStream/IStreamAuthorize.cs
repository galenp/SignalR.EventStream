using System.Security.Principal;

namespace Microsoft.AspNet.SignalR
{
    public interface IStreamAuthorize
    {
        bool Authorized(ref string clientId, IPrincipal user, string group);
    }
}