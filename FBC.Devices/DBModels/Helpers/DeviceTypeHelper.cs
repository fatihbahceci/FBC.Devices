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


        protected override void UpdateDataFor(DeviceType item)
        {
            db.DeviceTypes.Update(item);
            var exists = db.DeviceTypes.FirstOrDefault(x => x.DeviceTypeId == item.DeviceTypeId);
            if (exists != null)
            {
                db.Entry(exists).CurrentValues.SetValues(item);
            } else
            {
                throw new ArgumentNullException("All", "Not found (Update -> DeviceTypes)");
            }
        }

        public List<DeviceType> GetList(bool withEmpty)
        {
            var ret = CreateBaseQuery(true).ToList();
            if (withEmpty)
            {
                ret.Insert(0, new DeviceType() { DeviceTypeId = 0, Name = "<Not Selected>" });
            }
            return ret;
        }

        protected override IQueryable<DeviceType> getBaseQuery(params (string Key, string[] Values)[] extraParams)
        {
            return  db.DeviceTypes.AsQueryable();
        }
    }
}
