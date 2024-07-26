using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.ContentModel;

namespace tests;

public class TestAuthHandlerManager : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandlerManager(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Manager
        //http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier: c7abb08a-70e2-4e9c-b849-871774662800
        //http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name: manager@task.org
        //http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress: manager@task.org
        //AspNet.Identity.SecurityStamp: YNNB3VMWDIZPP2MRUYX63NQDWSFNGS6Z
        //http://schemas.microsoft.com/ws/2008/06/identity/claims/role: Managers
        //amr: pwd
        
        var claims = new[] {
            new Claim(ClaimTypes.Name, "manager@task.org"),
            new Claim(ClaimTypes.Email, "manager@task.org"),
            new Claim(ClaimTypes.NameIdentifier, "c7abb08a-70e2-4e9c-b849-871774662800"),
            new Claim(ClaimTypes.Role, "Managers"),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}