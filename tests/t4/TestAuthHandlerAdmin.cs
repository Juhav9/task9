using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace tests;

public class TestAuthHandlerAdmin : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandlerAdmin(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Admin
        // http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier: 3d489bfa-96a8-4505-acb7-7c3b18dff6c5
        // http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name: admin@task.org
        // http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress: admin@task.org
        // AspNet.Identity.SecurityStamp: L6YJLVWFNXIQWMWUJSBNFJEJ2K4HJLD7
        // http://schemas.microsoft.com/ws/2008/06/identity/claims/role: Admins
        // amr: pwd

        var claims = new[] {
            new Claim(ClaimTypes.Name, "admin@task.org"),
            new Claim(ClaimTypes.Email, "admin@task.org"),
            new Claim(ClaimTypes.NameIdentifier, "3d489bfa-96a8-4505-acb7-7c3b18dff6c5"),
            new Claim(ClaimTypes.Role, "Admins"),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}