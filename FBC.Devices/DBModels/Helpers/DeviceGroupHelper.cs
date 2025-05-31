using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.DBModels.Helpers
{
    public class DeviceGroupHelper : DBHelper<DeviceGroup>
    {
        public DeviceGroupHelper(DeviceDB parent, DB dibi) : base(parent, dibi)
        {

        }

        protected override void AddDataFor(DeviceGroup item)
        {
            db.DeviceGroups.Add(item);
        }

        protected override void DeleteDataFor(DeviceGroup item)
        {
            db.DeviceGroups.Remove(item);
        }

        protected override IQueryable<DeviceGroup> getBaseQuery(bool noTracking = true, params (string Key, string[] Values)[] extraParams)
        {
            var basex = noTracking ?
                db.DeviceGroups.AsNoTracking().AsQueryable() :
                db.DeviceGroups.AsQueryable();
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

        protected override bool IsNewData(DeviceGroup item)
        {
            return item.DeviceGroupId <= 0;
        }

        protected override void UpdateDataFor(DeviceGroup item)
        {
            db.DeviceGroups.Update(item);
        }

        public List<DeviceGroup> GetList(bool withEmpty)
        {
            var ret = getBaseQuery(true).ToList();
            if (withEmpty)
            {
                ret.Insert(0, new DeviceGroup() { DeviceGroupId = 0, Name = "<Not Selected>" });
            }
            return ret;
        }
    }
}
