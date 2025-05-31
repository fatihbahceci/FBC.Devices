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

        protected override IQueryable<DeviceType> getBaseQuery(bool noTracking = true, params (string Key, string[] Values)[] extraParams)
        {
            var basex = noTracking ?
                db.DeviceTypes.AsNoTracking().AsQueryable() :
                db.DeviceTypes.AsQueryable();
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
            return basex;
        }
    }
}
