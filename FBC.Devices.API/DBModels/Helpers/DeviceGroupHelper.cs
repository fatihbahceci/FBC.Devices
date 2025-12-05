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

        protected override IQueryable<DeviceGroup> getBaseQuery(params (string Key, string[] Values)[] extraParams)
        {
            return db.DeviceGroups.AsQueryable();
        }


        protected override void UpdateDataFor(DeviceGroup item)
        {
            db.DeviceGroups.Update(item);
            var exists = db.DeviceGroups.FirstOrDefault(x => x.DeviceGroupId == item.DeviceGroupId);
            if (exists != null)
            {
                db.Entry(exists).CurrentValues.SetValues(item);
            }
            else
            {
                throw new ArgumentNullException("All", "Not found (Update -> DeviceGroups)");
            }
        }

        public List<DeviceGroup> GetList(bool withEmpty)
        {
            var ret = CreateBaseQuery(true).ToList();
            if (withEmpty)
            {
                ret.Insert(0, new DeviceGroup() { DeviceGroupId = 0, Name = "<Not Selected>" });
            }
            return ret;
        }
    }
}
