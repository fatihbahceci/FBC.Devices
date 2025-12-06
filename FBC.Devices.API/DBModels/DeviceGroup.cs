using FBC.Devices.API.DBModels.Repository;
using System.ComponentModel.DataAnnotations;

namespace FBC.Devices.DBModels
{
    public class DeviceGroup : Entity<long>
    {
        public string Name { get; set; }
        public string? Description { get; set; }

        public DeviceGroup()
        {
            Name = "New Group";
        }

        public override void CheckDataFor(EntityOperation entityOperation, bool alsoValidate)
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