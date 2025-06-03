using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using FBC.Devices.DBModels;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace FBC.Devices.Services
{

    public class DummyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public DummyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // This is a dummy authentication handler that always returns an authenticated user with no claims.
            var identity = new ClaimsIdentity(); // anonymous identity
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "CustomScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public class FBCAuthenticationStateProvider : AuthenticationStateProvider
    {
        private DBUser? _currentUser;

        public Task<bool> LoginAsync(string username, string password)
        {
            using var db = new DB();
            string hashedPassword = C.Tools.ToMD5(password);
            var user = db.SysUsers.FirstOrDefault(u => u.UserName == username && u.Password == hashedPassword);

            if (user == null)
            {
                _currentUser = null;
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return Task.FromResult(false);
            }

            _currentUser = user;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return Task.FromResult(true);
        }

        public void Logout()
        {
            _currentUser = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsIdentity identity;

            if (_currentUser != null)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, _currentUser.UserName),
                new Claim("FullName", _currentUser.Name),
                new Claim("IsSysAdmin", _currentUser.IsSysAdmin.ToString())
            };

                if (!string.IsNullOrWhiteSpace(_currentUser.Roles))
                {
                    var roles = _currentUser.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var role in roles)
                        claims.Add(new Claim(ClaimTypes.Role, role));
                }

                identity = new ClaimsIdentity(claims, "FBCAuth");
            }
            else
            {
                identity = new ClaimsIdentity(); // anonymous
            }

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
    }

    public class FBCAuthorizationHandler : AuthorizationHandler<RolesAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RolesAuthorizationRequirement requirement)
        {
            if (context.User == null || !context.User.Identity?.IsAuthenticated == true)
                return Task.CompletedTask;

            var roles = requirement.AllowedRoles;

            // Override for SysAdmin role
            if (context.User.IsInRole(C.UserRoles.SysAdmin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Check if the user is in any of the allowed roles
            foreach (var role in roles)
            {
                if (context.User.IsInRole(role))
                {
                    context.Succeed(requirement);
                    break;
                }
            }

            return Task.CompletedTask;
        }
    }
}
