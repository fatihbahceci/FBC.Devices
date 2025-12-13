using FBC.DBRepository;
using System.ComponentModel.DataAnnotations.Schema;

namespace FBC.Devices.DBModels
{
    public interface IDBUserRepository : IAsyncRepository<DBUser, long>;

    public class DBUserRepository : EFRepositoryBase<DBUser, long, DB>, IDBUserRepository
    {
        public DBUserRepository(DB context) : base(context)
        {

        }
    }
    public class DBUser : EntityBase<DBUser>
    {
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

        public override void CheckDataFor(EntityOperation entityOperation, bool alsoValidate, IQueryable<DBUser> query)
        {
            if (entityOperation != EntityOperation.Delete)
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
            }

            if (alsoValidate)
            {
                if (entityOperation != EntityOperation.Delete)
                {
                    if (string.IsNullOrWhiteSpace(UserName))
                        throw new ArgumentException("UserName cannot be empty", nameof(UserName));
                    if (string.IsNullOrWhiteSpace(Password))
                        throw new ArgumentException("Password cannot be empty", nameof(Password));
                    if (string.IsNullOrWhiteSpace(Name))
                        throw new ArgumentException("Name cannot be empty", nameof(Name));
                }
                switch (entityOperation)
                {
                    case EntityOperation.Create:
                        if (query.Any(x => x.UserName == UserName))
                        {
                            throw new ArgumentException($"User with username '{UserName}' already exists.", nameof(UserName));
                        }
                        break;
                    case EntityOperation.Update:
                        if (query.Any(x => x.UserName == UserName && x.Id != Id))
                        {
                            throw new ArgumentException($"User with username '{UserName}' already exists.", nameof(UserName));
                        }
                        // Check if we are removing SysAdmin rights from the last SysAdmin user
                        if (!IsSysAdmin)
                        {
                            // Find the current data for this user
                            var exists = query.FirstOrDefault(x => x.Id == Id);
                            if (exists != null)
                            {
                                //Is the existing user was a SysAdmin and there is no other SysAdmin user?
                                if (exists.IsSysAdmin && query.Count(x => x.IsSysAdmin) <= 1)
                                {
                                    throw new InvalidOperationException("You trying to remove SysAdmin rights from the last SysAdmin user. At least one SysAdmin is required.");
                                }
                            }
                        }
                        break;
                    case EntityOperation.Delete:
                        if (IsSysAdmin && query.Count(x => x.IsSysAdmin) <= 1)
                        {
                            throw new InvalidOperationException("Cannot delete the last SysAdmin user. At least one SysAdmin is required.");
                        }
                        break;

                }
            }
        }
    }
}