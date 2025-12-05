using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.DBModels.Helpers
{
    public class DeviceAddrHelper : DBHelper<DeviceAddr>
    {
        public DeviceAddrHelper(DeviceDB parent, DB dibi) : base(parent, dibi)
        {
        }

        protected override void AddDataFor(DeviceAddr item)
        {
            item.AdjustData();
            db.DeviceAddresses.Add(item);
        }

        protected override void DeleteDataFor(DeviceAddr item)
        {
            db.DeviceAddresses.RemoveRange(db.DeviceAddresses.Where(x => x.DeviceAddrId == item.DeviceAddrId));
            //db.DeviceAddrs.Remove(item);
        }

        protected override IQueryable<DeviceAddr> getBaseQuery((string Key, string[] Values)[] extraParams)
        {
            return db.DeviceAddresses.AsQueryable();
        }

        protected override void UpdateDataFor(DeviceAddr item)
        {
            item.AdjustData();
            //db.DeviceAddresses.Update(item);
            var exists = db.DeviceAddresses.FirstOrDefault(x => x.DeviceAddrId == item.DeviceAddrId);
            if (exists != null)
            {
                db.Entry(exists).CurrentValues.SetValues(item);
            }
            else
            {
                throw new ArgumentNullException("All", "Not found (Update -> DeviceAddresses)");
            }
        }
    }
}
