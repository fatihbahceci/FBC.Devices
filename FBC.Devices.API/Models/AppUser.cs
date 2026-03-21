using System.ComponentModel.DataAnnotations.Schema;
using FBC.DBRepository;

namespace FBC.Devices.API.Models;

public class AppUser : Entity<int, AppUser>
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    [NotMapped]
    public string? NewPassword { get; set; }

    public bool IsSysAdmin { get; set; }
    public string Roles { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public string[] GetRoles()
    {
        return (Roles?.Trim() ?? "").Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public void SetRoles(IEnumerable<string> roles)
    {
        Roles = roles == null
            ? string.Empty
            : string.Join(",", roles.Select(r => r.Trim()).Where(r => !string.IsNullOrWhiteSpace(r)));
    }

    public void AdjustData(bool validate)
    {
        var roles = GetRoles().ToList();
        if (IsSysAdmin && !roles.Contains(Constants.UserRoles.SysAdmin))
            roles.Add(Constants.UserRoles.SysAdmin);
        else if (!IsSysAdmin && roles.Contains(Constants.UserRoles.SysAdmin))
            roles.Remove(Constants.UserRoles.SysAdmin);
        SetRoles(roles);

        if (!string.IsNullOrWhiteSpace(NewPassword))
        {
            Password = Constants.Tools.ToMD5(NewPassword);
            NewPassword = null;
        }

        if (validate)
        {
            if (string.IsNullOrWhiteSpace(UserName))
                throw new ArgumentException("UserName cannot be empty", nameof(UserName));
            if (string.IsNullOrWhiteSpace(Password))
                throw new ArgumentException("Password cannot be empty", nameof(Password));
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("Name cannot be empty", nameof(Name));
        }
    }
}
