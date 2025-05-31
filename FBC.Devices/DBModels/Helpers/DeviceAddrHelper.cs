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

        protected override IQueryable<DeviceAddr> getBaseQuery(bool noTracking = true, params (string Key, string[] Values)[] extraParams)
        {
            var basex = noTracking ?
                db.DeviceAddresses.AsNoTracking().AsQueryable() :
                db.DeviceAddresses.AsQueryable();
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

        protected override bool IsNewData(DeviceAddr item)
        {
            return item.DeviceId <= 0;
        }

        protected override void UpdateDataFor(DeviceAddr item)
        {
            item.AdjustData();
            db.DeviceAddresses.Update(item);
        }
    }
}
