using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.DBModels.Helpers
{
    public class DeviceHelper : DBHelper<Device>
    {
        public DeviceHelper(DeviceDB parent, DB dibi) : base(parent, dibi)
        {
        }

        protected override void AddDataFor(Device item)
        {
            item.AdjustData(true);
            db.Devices.Add(item);
        }

        protected override void DeleteDataFor(Device item)
        {
            if (item.DeviceAddresses?.Any() == true)
            {
                foreach (var i in item.DeviceAddresses)
                {
                    Parent.DeviceAddresses.DeleteData(i);
                }
            }
            db.Devices.RemoveRange(db.Devices.Where(x => x.DeviceId == item.DeviceId));
        }

        protected override IQueryable<Device> getBaseQuery( params (string Key, string[] Values)[] extraParams)
        {
            return db.Devices.AsQueryable();
        }


        protected override void UpdateDataFor(Device item)
        {
            item.AdjustData(true);
            if (item.DeviceAddresses?.Any() == true)
            {
                foreach (var i in item.DeviceAddresses)
                {
                    Parent.DeviceAddresses.AddOrUpdateData(i);
                }
                item.DeviceAddresses?.Clear();
            }
            var exist = db.Devices.Find(item.DeviceId);
            if (exist == null)
            {
                throw new ArgumentNullException("All", "Not found (Update -> Devices)");
            }
            db.Entry(exist).CurrentValues.SetValues(item);
        }
    }
}
