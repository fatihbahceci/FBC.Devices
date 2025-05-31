using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.DBModels.Helpers
{
    public class DeviceTypeHelper : DBHelper<DeviceType>
    {
        public DeviceTypeHelper(DeviceDB parent, DB dibi) : base(parent, dibi)
        {
        }

        protected override void AddDataFor(DeviceType item)
        {
            db.DeviceTypes.Add(item);
        }

        protected override void DeleteDataFor(DeviceType item)
        {
            db.DeviceTypes.Remove(item);
        }

        protected override IQueryable<DeviceType> getBaseQuery(bool noTracking, object? extraParams = null)
        {
            var basex = noTracking ?
                db.DeviceTypes.AsNoTracking().AsQueryable() :
                db.DeviceTypes.AsQueryable();
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

        protected override bool IsNewData(DeviceType item)
        {
            return item.DeviceTypeId <= 0;
        }

        protected override void UpdateDataFor(DeviceType item)
        {
            db.DeviceTypes.Update(item);
        }

        public List<DeviceType> GetList(bool withEmpty)
        {
            var ret = getBaseQuery(true).ToList();
            if (withEmpty)
            {
                ret.Insert(0, new DeviceType() { DeviceTypeId = 0, Name = "<Not Selected>" });
            }
            return ret;
        }
    }
}
