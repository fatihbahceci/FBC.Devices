using FBC.Devices.API.DBModels.Repository;
using System.ComponentModel.DataAnnotations;

namespace FBC.Devices.DBModels
{
    /// <summary>
    /// Firewall, VM, PC, Switch, Router, etc.
    /// </summary>
    public class DeviceType : Entity<long, DeviceType>
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public DeviceType()
        {
            Name = "New Type";
        }

        public override void CheckDataFor(EntityOperation entityOperation, bool alsoValidate, IQueryable<DeviceType> query)
        {
            if (alsoValidate)
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    throw new ValidationException("DeviceType Name cannot be empty.");
                }
            }
        }
    }
}