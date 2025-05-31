using System.ComponentModel.DataAnnotations;

namespace FBC.Devices.DBModels
{
    public class DeviceGroup
    {
        [Key]
        public int DeviceGroupId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public DeviceGroup()
        {
            Name = "New Group";
        }
    }
}