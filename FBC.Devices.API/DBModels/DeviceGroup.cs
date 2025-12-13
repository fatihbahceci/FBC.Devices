using FBC.DBRepository;
using System.ComponentModel.DataAnnotations;

namespace FBC.Devices.DBModels
{
    public class DeviceGroup : EntityBase<DeviceGroup>
    {
        public string Name { get; set; }
        public string? Description { get; set; }

        public DeviceGroup()
        {
            Name = "New Group";
        }

        public override void CheckDataFor(EntityOperation entityOperation, bool alsoValidate, IQueryable<DeviceGroup> query)
        {
            if (alsoValidate)
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    throw new ValidationException("DeviceGroup Name cannot be empty.");
                }
            }
        }
    }
}