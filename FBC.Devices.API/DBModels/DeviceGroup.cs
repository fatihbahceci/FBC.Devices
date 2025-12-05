using FBC.Devices.DBModels.Helpers;
using System.ComponentModel.DataAnnotations;

namespace FBC.Devices.DBModels
{
    public class DeviceGroup : IHasPrimaryKey
    {
        [Key]
        public int DeviceGroupId { get; set; }
        public int PrimaryKeyId => DeviceGroupId;
        public string Name { get; set; }
        public string? Description { get; set; }

        public DeviceGroup()
        {
            Name = "New Group";
        }
    }
}