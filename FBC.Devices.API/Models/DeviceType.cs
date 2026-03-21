using FBC.DBRepository;
using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.API.Models;

public class DeviceType : APIBaseEntity<DeviceType>
{
    public string Name { get; set; } = "New Type";
    public string? Description { get; set; }

    public override async Task CheckDataForAsync(EntityOperation operation, bool alsoValidate, IQueryable<DeviceType> query)
    {
        if (alsoValidate)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("Name cannot be empty", nameof(Name));
            if (query != null)
            {
                switch (operation)
                {
                    case EntityOperation.Create:
                    case EntityOperation.Update:
                        var exists = await query.AnyAsync(x => x.Id != Id && x.Name == Name);
                        if (exists)
                            throw new ArgumentException($"A device type with the name '{Name}' already exists.", nameof(Name));
                        break;
                }
            }
        }
    }
}
