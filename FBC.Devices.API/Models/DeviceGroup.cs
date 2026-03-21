using FBC.DBRepository;
using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.API.Models;

public class DeviceGroup : APIBaseEntity<DeviceGroup>
{
    public string Name { get; set; } = "New Group";
    public string? Description { get; set; }

    public override async Task CheckDataForAsync(EntityOperation operation, bool alsoValidate, IQueryable<DeviceGroup> query)
    {
        if (alsoValidate)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("Name cannot be empty", nameof(Name));
            if (query != null)
            {
                var exists = await query.AnyAsync(x => x.Id != Id && x.Name == Name);
                if (exists)
                    throw new ArgumentException("A group with the same name already exists", nameof(Name));
            }
        }

    }
}
