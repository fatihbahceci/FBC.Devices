using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;

namespace FBC.Devices.Models
{
    public class ACGetListRequest
    {
        public string Filter { get; set; }
        public string OrderBy { get; set; }
        /// <summary>
        /// Skip records
        /// </summary>
        public int Skip { get; set; }
        /// <summary>
        /// Maximum record count per request
        /// </summary>
        public int Take { get; set; }

        public ACGetListRequest(string filter, string orderBy, int? skip, int? take)
        {
            this.Filter = filter;
            this.OrderBy = orderBy;
            this.Skip = skip ?? 0;
            this.Take = take ?? 0;

        }
        public ACGetListRequest(string filter, string orderBy) : this(filter, orderBy, 0, 0)
        {
            this.Filter = filter;
            this.OrderBy = orderBy;
        }

        public ACGetListRequest() : this("", "", 0, 0)
        {

        }
    }


    public abstract class APIDBContextHelper<TDataTable, TDatabase>
        where TDataTable : class
        where TDatabase : DbContext, new()
    {
        private TDatabase _db;

        public APIDBContextHelper(TDatabase dibi)
        {
            this._db = dibi;
        }

        protected TDatabase db
        {
            get
            {
                return _db;
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="u"></param>
        ///// <returns>WARNING! it can be null</returns>
        //protected IQueryable<MeterCard> getBaseQueryForMeterCard(VMLoggedUser u)
        //{
        //    //Console.WriteLine(new String('.',500));
        //    var meterCardQuery = from x in db.MeterCard.AsNoTracking() where x.Deleted == false select x;

        //    if (u != null)
        //    {
        //        if (!u.HasAllMeterGroupRights)
        //        {
        //            var rights = new List<string>();
        //            if (u.UserMeterGroupRights != null && u.UserMeterGroupRights.Any())
        //            {
        //                rights.AddRange(u.UserMeterGroupRights);
        //            }

        //            if (u.HasNonGroupedMeterRights)
        //            {
        //                rights.Add(null);
        //                rights.Add("");
        //            }
        //            if (rights.Any())
        //            {
        //                meterCardQuery = meterCardQuery.Where(x => rights.Contains(x.MeterGroupName));
        //            }
        //            else
        //            {
        //                //do not return any meter
        //                //q = q.Where(x => false);
        //                //don't query if user has no rights, just return null
        //                return null;
        //            }
        //        }
        //    }
        //    return meterCardQuery;
        //}

        /// <summary>
        /// Other queries call this method first.If it returns null then the calling query also returns null.
        /// </summary>
        /// <param name="extraParams">
        /// For example: ("UserRights", ["Admin"]))
        /// </param>
        /// <returns></returns>
        protected abstract IQueryable<TDataTable> getBaseQuery((string Key, string[] Values)[] extraParams);
        /// <summary>
        /// Creates a base query for the data table.
        /// </summary>
        /// <param name="noTracking">
        /// true: give a query read only (for performance and keep data read only)<br />
        /// false: give an edit-open query for updating data
        /// </param>
        /// <param name="extraParams">
        /// For example: ("UserRights", ["Admin"]), ("Include", ["Table1", "Table2"])
        /// </param>
        /// <returns></returns>
        protected IQueryable<TDataTable> CreateBaseQuery(bool noTracking = true, params (string Key, string[] Values)[] extraParams)
        {
            var query = getBaseQuery(extraParams.Where(x => x.Key != C.DBQ.Ex.Include).ToArray());
            if (query == null)
            {
                //if query is null, return empty query
                return Enumerable.Empty<TDataTable>().AsQueryable();
            }
            foreach (var param in extraParams)
            {
                switch (param.Key)
                {
                    case C.DBQ.Ex.Include:
                        if (param.Values != null && param.Values.Length > 0)
                        {
                            foreach (var i in param.Values)
                            {
                                query = query.Include(i);
                            }
                        }
                        break;
                    //default:
                    //    throw new ArgumentException($"Unknown parameter key: {param.Key}");
                }
            }

            if (noTracking)
            {
                query = query.AsNoTracking();
            }
            return query;
        }

        private IQueryable<TDataTable> applyFilter(IQueryable<TDataTable> q, ACGetListRequest r)
        {
            if (!string.IsNullOrEmpty(r.Filter))
            {
                q = q.Where(r.Filter);
            }

            if (!string.IsNullOrEmpty(r.OrderBy))
            {
                //    // Sort via the OrderBy method
                q = q.OrderBy(r.OrderBy);
            }

            return q;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="aq"></param>
        /// <param name="noTracking">
        /// true: give a query read only (for performance and keep data read only)<br />
        /// false: give an edit-open query for updating data
        /// </param>
        /// <param name="extraParams">
        /// For example: ("UserRights", ["Admin"]), ("Include", ["Table1", "Table2"])
        /// </param>

        /// <returns></returns>
        public TDataTable? FirstOrDefault(ACGetListRequest aq, bool noTracking = true, params (string Key, string[] Values)[] extraParams)
        {
            var q = CreateBaseQuery(noTracking, extraParams);
            q = applyFilter(q, aq);
            return q.FirstOrDefault();
        }

        //public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="noTracking">
        /// true: give a query read only (for performance and keep data read only)<br />
        /// false: give an edit-open query for updating data
        /// </param>
        /// <param name="extraParams">
        /// For example: ("UserRights", ["Admin"]), ("Include", ["Table1", "Table2"])
        /// </param>
        /// <returns></returns>
        public TDataTable? FirstOrDefault(Expression<Func<TDataTable, bool>> predicate, bool noTracking = true, params (string Key, string[] Values)[] extraParams)
        {
            var q = CreateBaseQuery(noTracking, extraParams);
            q = q.Where(predicate);
            return q.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aq"></param>
        /// <param name="noTracking">
        /// true: give a query read only (for performance and keep data read only)<br />
        /// false: give an edit-open query for updating data
        /// </param>
        /// <param name="extraParams">
        /// For example: ("UserRights", ["Admin"]), ("Include", ["Table1", "Table2"])
        /// </param>
        /// <returns></returns>
        public async Task<TDataTable?> FirstOrDefaultAsync(ACGetListRequest aq, bool noTracking = true, params (string Key, string[] Values)[] extraParams)
        {
            var q = CreateBaseQuery(noTracking, extraParams);
            q = applyFilter(q, aq);
            return await q.FirstOrDefaultAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="noTracking">
        /// true: give a query read only (for performance and keep data read only)<br />
        /// false: give an edit-open query for updating data
        /// </param>
        /// <param name="extraParams">
        /// For example: ("UserRights", ["Admin"]), ("Include", ["Table1", "Table2"])
        /// </param>
        /// <returns></returns>
        public async Task<TDataTable?> FirstOrDefaultAsync(Expression<Func<TDataTable, bool>> predicate, bool noTracking = true, params (string Key, string[] Values)[] extraParams)
        {
            var q = CreateBaseQuery(noTracking, extraParams);
            q = q.Where(predicate);
            return await q.FirstOrDefaultAsync();
        }
        /// <summary>
        /// if user not set, results all meters with conditions.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="extraParams">
        /// For example: ("UserRights", ["Admin"]), ("Include", ["Table1", "Table2"])
        /// </param>
        /// <returns></returns>
        public List<TDataTable> ToList(Expression<Func<TDataTable, bool>> e, params (string Key, string[] Values)[] extraParams)
        {
            var q = CreateBaseQuery(true, extraParams);
            if (q == null)
            {
                //if query is null, return empty list
                return new List<TDataTable>();
            }
            else
            {
                if (e != null) q = q.Where(e);
                return q.ToList();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="extraParams">
        /// For example: ("UserRights", ["Admin"]), ("Include", ["Table1", "Table2"])
        /// </param>
        /// <returns></returns>
        public DataList<TDataTable> ToList(ACGetListRequest args, params (string Key, string[] Values)[] extraParams)
        {
            var query = CreateBaseQuery(true, extraParams);
            if (query != null)
            {
                var result = new DataList<TDataTable>()
                {
                    NonFilteredCount = query.Count(),
                };
                query = applyFilter(query, args);
                //result.Data = query.Skip(args.Skip.Value).Take(args.Top.Value).ToList().Select(x => x?.Convert()).ToList();
                result.Data = query.Skip(args.Skip).Take(args.Take).ToList();

                return result;
            }
            else
            {
                return DataList<TDataTable>.Empty;
            }



        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="extraParams">
        /// For example: ("UserRights", ["Admin"]), ("Include", ["Table1", "Table2"])
        /// </param>
        /// <returns></returns>
        public async Task<List<TDataTable>> ToListAsync(Expression<Func<TDataTable, bool>> e, params (string Key, string[] Values)[] extraParams)
        {
            var q = CreateBaseQuery(true, extraParams);
            if (q == null)
            {
                return new List<TDataTable>();
            }
            else
            {
                if (e != null) q = q.Where(e);
                return await q.ToListAsync();
            }
        }
    }

    public class APIResponse<TResponseStatus, TResultData>
    {
        public TResponseStatus? ResponseStatus { get; set; }

        public TResultData? Data { get; set; }

        public APIResponse(TResponseStatus? responseStatus, TResultData? data)
        {
            ResponseStatus = responseStatus;
            Data = data;
        }
        public APIResponse(TResponseStatus responseStatus) : this(responseStatus, default)
        {

        }

        public APIResponse() : this(default, default)
        {

        }


    }

    public class DataList<T> : APIResponse<int, List<T>>
    {
        public DataList()
        {
            this.ResponseStatus = 0;
        }

        public static DataList<T> Empty
        {
            get => new DataList<T>()
            {
                NonFilteredCount = 0
            };
        }

        /// <summary>
        /// All data count
        /// </summary>
        public int NonFilteredCount { get; set; }

    }
}
