using FBC.DBRepository;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FBC.Devices.API.Models;

public class AddrType : APIBaseEntity<AddrType>
{
    public string Name { get; set; } = "New Address Type";

    public override async Task CheckDataForAsync(EntityOperation operation, bool alsoValidate, IQueryable<AddrType> query)
    {
        if (alsoValidate)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ValidationException("Name is required.");
            if (query != null)
                switch (operation)
                {
                    case EntityOperation.Create:
                    case EntityOperation.Update:
                        var exists = await query.AnyAsync(a => a.Name == Name && a.Id != Id);
                        if (exists)
                            throw new ValidationException("An address type with the same name already exists.");
                        break;
                }

        }
    }
}
