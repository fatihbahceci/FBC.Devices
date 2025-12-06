namespace FBC.Devices.API.DBModels.Repository;

public interface IQuery<T>
{
    IQueryable<T> Query();
}

