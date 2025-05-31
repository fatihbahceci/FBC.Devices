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

        protected override IQueryable<Device> getBaseQuery(bool noTracking, object? extraParams = null)
        {
            var basex = noTracking ?
                db.Devices.AsNoTracking().AsQueryable() :
                db.Devices.AsQueryable();
            if (extraParams != null && extraParams is IEnumerable<string>)
            {
                foreach (var i in (IEnumerable<string>)extraParams)
                {
                    basex = basex.Include(i);
                }
            }
            //.Include(x => x.Kariyer);
            return basex;
        }

        protected override bool IsNewData(Device item)
        {
            return item.DeviceId <= 0;
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
                throw new ArgumentNullException("All", "Not found (Update)");
            }
            db.Entry(exist).CurrentValues.SetValues(item);
        }
    }
}
