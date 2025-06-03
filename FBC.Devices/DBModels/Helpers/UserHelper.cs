namespace FBC.Devices.DBModels.Helpers
{
    public class UserHelper : DBHelper<DBUser>
    {
        public UserHelper(DeviceDB parent, DB dibi) : base(parent, dibi)
        {
        }
        protected override void AddDataFor(DBUser item)
        {
            item.AdjustData(true);
            if (db.SysUsers.Any(x => x.UserName == item.UserName))
            {
                throw new ArgumentException($"User with username '{item.UserName}' already exists.", nameof(item.UserName));
            }
            db.SysUsers.Add(item);
        }
        protected override void DeleteDataFor(DBUser item)
        {
            if (item.IsSysAdmin && db.SysUsers.Count(x => x.IsSysAdmin) <= 1)
            {
                throw new InvalidOperationException("Cannot delete the last SysAdmin user. At least one SysAdmin is required.");
            }
            db.SysUsers.Remove(item);
        }
        protected override IQueryable<DBUser> getBaseQuery(params (string Key, string[] Values)[] extraParams)
        {
            return db.SysUsers.AsQueryable();
        }
        protected override void UpdateDataFor(DBUser item)
        {
            item.AdjustData(true);
            if (db.SysUsers.Any(x => x.UserName == item.UserName && x.UserId != item.UserId))
            {
                throw new ArgumentException($"User with username '{item.UserName}' already exists.", nameof(item.UserName));
            }
            var exists = db.SysUsers.FirstOrDefault(x => x.UserId == item.UserId);
            if (exists != null)
            {
                if (exists.IsSysAdmin && !item.IsSysAdmin && db.SysUsers.Count(x => x.IsSysAdmin) <= 1)
                {
                    throw new InvalidOperationException("You trying to remove SysAdmin rights from the last SysAdmin user. At least one SysAdmin is required.");
                }
                db.Entry(exists).CurrentValues.SetValues(item);
            }
            else
            {
                throw new ArgumentNullException("All", "Not found (Update -> SysUsers)");
            }
        }
    }
}
