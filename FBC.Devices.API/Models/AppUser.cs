using FBC.DBRepository;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FBC.Devices.API.Models;

public class AppUser : APIBaseEntity<AppUser>
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

    public override async Task CheckDataForAsync(EntityOperation operation, bool alsoValidate, IQueryable<AppUser> query)
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

        if (alsoValidate)
        {
            if (string.IsNullOrWhiteSpace(UserName))
                throw new ArgumentException("UserName cannot be empty", nameof(UserName));
            if (string.IsNullOrWhiteSpace(Password))
                throw new ArgumentException("Password cannot be empty", nameof(Password));
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("Name cannot be empty", nameof(Name));
            if (query != null)
            {
                switch (operation)
                {
                    case EntityOperation.Create:
                    case EntityOperation.Update:
                        var exists = await query.AnyAsync(u => u.UserName == UserName && u.Id != Id);
                        if (exists)
                        {
                            throw new ArgumentException("UserName already exists", nameof(UserName));
                        }
                        if (operation == EntityOperation.Update)
                        {
                            var current = await query.FirstOrDefaultAsync(u => u.Id == Id);
                            if (current != null && current.IsSysAdmin && !IsSysAdmin)
                            {
                                var sysAdminCount = await query.CountAsync(u => u.IsSysAdmin);
                                if (sysAdminCount <= 1)
                                {
                                    throw new InvalidOperationException("Cannot remove SysAdmin role from the last SysAdmin user");
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}
