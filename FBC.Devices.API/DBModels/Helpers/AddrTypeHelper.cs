using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.DBModels.Helpers
{
    public class AddrTypeHelper : DBHelper<AddrType>
    {
        public AddrTypeHelper(DeviceDB parent, DB dibi) : base(parent, dibi)
        {
        }

        protected override void AddDataFor(AddrType item)
        {
            db.AddrTypes.Add(item);
        }

        protected override void DeleteDataFor(AddrType item)
        {
            db.AddrTypes.Remove(item);
        }

        protected override IQueryable<AddrType> getBaseQuery(params (string Key, string[] Values)[] extraParams)
        {
            return db.AddrTypes.AsQueryable();
        }

        protected override void UpdateDataFor(AddrType item)
        {
            //db.AddrTypes.Update(item);
            var exists = db.AddrTypes.FirstOrDefault(x => x.AddrTypeId == item.AddrTypeId);
            if (exists != null)
            {
                db.Entry(exists).CurrentValues.SetValues(item);
            }
            else
            {
                throw new ArgumentNullException("All", "Not found (Update -> AddrTypes)");
            }
        }

        public List<AddrType> GetList(bool withEmpty)
        {
            var ret = CreateBaseQuery(true).ToList();
            if (withEmpty)
            {
                ret.Insert(0, new AddrType() { AddrTypeId = 0, Name = "<Not Selected>" });
            }
            return ret;
        }
    }
}
