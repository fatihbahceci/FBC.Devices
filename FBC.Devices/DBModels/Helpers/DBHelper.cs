using FBC.Devices.Models;

namespace FBC.Devices.DBModels.Helpers
{

    public abstract class DBHelper<TDataTable> : APIDBContextHelper<TDataTable, DB> where TDataTable : class
    {
        private DeviceDB manager;

        public DeviceDB Parent { get => manager; }
        protected DBHelper(DeviceDB manager, DB dibi) : base(dibi)
        {
            this.manager = manager;
        }

        //        private static T Cast<T>(TDataTable item) where T : class
        //        {
        //            //return item != null? item as T: default(T);
        //#pragma warning disable CS8603 // Possible null reference return.
        //            return item as T;
        //#pragma warning restore CS8603 // Possible null reference return.
        //            //return (T)Convert.ChangeType(item, typeof(T));
        //        }

        //        private static T Cast<T, TSource>(TSource item) where T : class
        //        {
        //            //return item != null? item as T: default(T);
        //#pragma warning disable CS8603 // Possible null reference return.
        //            return item as T;
        //#pragma warning restore CS8603 // Possible null reference return.
        //            //return (T)Convert.ChangeType(item, typeof(T));
        //        }

        internal void AddData(TDataTable item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("All", "Null object (Add)");
            }
            AddDataFor(item);
            SaveChanges();
        }
        internal void UpdateData(TDataTable item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("All", "Null object (Update)");
            }
            UpdateDataFor(item);
            SaveChanges();
        }
        internal void AddOrUpdateData(TDataTable item)
        {

            if (item == null)
            {
                throw new ArgumentNullException("All", "Null object (AddOrUpdate)");
            }
            if (IsNewData(item))
            {
                AddDataFor(item);
            }
            else
            {
                UpdateDataFor(item);
            }
            SaveChanges();
        }

        internal void DeleteData(TDataTable item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("All", "Null object (Delete)");
            }
            DeleteDataFor(item);
            SaveChanges();
        }
        protected abstract bool IsNewData(TDataTable item);
        protected abstract void AddDataFor(TDataTable item);
        protected abstract void UpdateDataFor(TDataTable item);
        protected abstract void DeleteDataFor(TDataTable item);


        internal void SaveChanges()
        {
            db.SaveChanges();
        }
    }
}
