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

        protected override IQueryable<DeviceAddr> getBaseQuery(bool noTracking, object? extraParams = null)
        {
            var basex = noTracking ?
                db.DeviceAddresses.AsNoTracking().AsQueryable() :
                db.DeviceAddresses.AsQueryable();
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
