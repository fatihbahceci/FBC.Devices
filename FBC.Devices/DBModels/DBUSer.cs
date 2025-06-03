using FBC.Devices.DBModels.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FBC.Devices.DBModels
{
    public class DBUser : IHasPrimaryKey
    {
        [Key]
        public int UserId { get; set; }
        public int PrimaryKeyId => UserId;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// New password for the user. This is not stored in the database directly.
        /// Not hashed until AdjustData is called.
        /// </summary>
        [NotMapped]
        public string? NewPassword { get; set; }
        public bool IsSysAdmin { get; set; } = false;
        /// <summary>
        /// Comma separated list of roles, e.g. "Admin,User,Viewer Or CRUD operations like "Create,Read,Update,Delete" or page rules. 
        /// </summary>
        public string Roles { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string[] GetRoles()
        {
            return (Roles?.Trim() ?? "").Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        public void SetRoles(IEnumerable<string> roles)
        {
            if (roles == null)
            {
                Roles = string.Empty;
            }
            else
            {
                Roles = string.Join(",", roles.Select(r => r.Trim()).Where(r => !string.IsNullOrWhiteSpace(r)));
            }
        }
        public void AdjustData(bool validate)
        {
            var roles = GetRoles().ToList();
            if (IsSysAdmin && !roles.Contains(C.UserRoles.SysAdmin))
            {
                roles.Add(C.UserRoles.SysAdmin);
            }
            else if (!IsSysAdmin && roles.Contains(C.UserRoles.SysAdmin))
            {
                roles.Remove(C.UserRoles.SysAdmin);
            }
            SetRoles(roles);

            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                Password = C.Tools.ToMD5(NewPassword);
                NewPassword = null; // Clear the new password after hashing
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
}