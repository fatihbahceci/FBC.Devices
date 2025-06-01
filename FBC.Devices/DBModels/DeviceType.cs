using FBC.Devices.DBModels.Helpers;
using System.ComponentModel.DataAnnotations;

namespace FBC.Devices.DBModels
{
    /// <summary>
    /// Firewall, VM, PC, Switch, Router, etc.
    /// </summary>
    public class DeviceType : IHasPrimaryKey
    {
        [Key]
        public int DeviceTypeId { get; set; }
        public int PrimaryKeyId => DeviceTypeId;
        public string Name { get; set; }
        public string? Description { get; set; }
        public DeviceType()
        {
            Name = "New Type";
        }
    }
}