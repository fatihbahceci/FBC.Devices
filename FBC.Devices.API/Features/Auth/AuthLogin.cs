using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FBC.Devices.API.Data;
using FBC.Mediator;
using Microsoft.IdentityModel.Tokens;

namespace FBC.Devices.API.Features.Auth;

public sealed class AuthLogin
{
    public record Command(string Username, string Password) : IRequest<LoginResponse?>;

    public record LoginResponse(string Token, string UserName, string Name, bool IsSysAdmin, string[] Roles);

    internal sealed class Handler(AppDbContext db, IConfiguration config)
        : IRequestHandler<Command, LoginResponse?>
    {
        public async Task<LoginResponse?> Handle(Command request, CancellationToken token = default)
        {
            var hashedPassword = Constants.Tools.ToMD5(request.Password);
            var user = db.SysUsers.FirstOrDefault(
                u => u.UserName == request.Username && u.Password == hashedPassword);

            if (user == null)
                return null;

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new("FullName", user.Name),
                new("IsSysAdmin", user.IsSysAdmin.ToString()),
                new("UserId", user.Id.ToString())
            };

            var roles = user.GetRoles();
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresInHours = int.Parse(config["Jwt:ExpiresInHours"] ?? "24");

            var jwtToken = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiresInHours),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return new LoginResponse(tokenString, user.UserName, user.Name, user.IsSysAdmin, roles);
        }
    }
}
