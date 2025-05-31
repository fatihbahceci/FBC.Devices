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

        protected override IQueryable<Device> getBaseQuery(bool noTracking = true, params (string Key, string[] Values)[] extraParams)
        {
            var basex = noTracking ?
                db.Devices.AsNoTracking().AsQueryable() :
                db.Devices.AsQueryable();
            foreach (var param in extraParams)
            {
                switch (param.Key)
                {
                    case C.DBQ.Ex.Include:
                        if (param.Values != null && param.Values.Length > 0)
                        {
                            foreach (var i in param.Values)
                            {
                                basex = basex.Include(i);
                            }
                        }
                        break;
                    default:
                        throw new ArgumentException($"Unknown parameter key: {param.Key}");
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
