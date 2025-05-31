using System.ComponentModel.DataAnnotations;

namespace FBC.Devices.DBModels
{
    /// <summary>
    /// Firewall, VM, PC, Switch, Router, etc.
    /// </summary>
    public class DeviceType
    {
        [Key]
        public int DeviceTypeId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DeviceType()
        {
            Name = "New Type";
        }
    }
}