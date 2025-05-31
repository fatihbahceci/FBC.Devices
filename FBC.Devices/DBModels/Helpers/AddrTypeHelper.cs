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

        protected override IQueryable<AddrType> getBaseQuery(bool noTracking, object? extraParams = null)
        {
            var basex = noTracking ?
                db.AddrTypes.AsNoTracking().AsQueryable() :
                db.AddrTypes.AsQueryable();
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

        protected override bool IsNewData(AddrType item)
        {
            return item.AddrTypeId <= 0;
        }

        protected override void UpdateDataFor(AddrType item)
        {
            db.AddrTypes.Update(item);
        }

        public List<AddrType> GetList(bool withEmpty)
        {
            var ret = getBaseQuery(true).ToList();
            if (withEmpty)
            {
                ret.Insert(0, new AddrType() { AddrTypeId = 0, Name = "<Not Selected>" });
            }
            return ret;
        }
    }
}
