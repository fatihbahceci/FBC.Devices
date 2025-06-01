using FBC.Devices.DBModels.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FBC.Devices.DBModels
{
    public class Device : IHasPrimaryKey
    {
        [Key]
        public int DeviceId { get; set; }
        public int PrimaryKeyId => DeviceId;
        public string Name { get; set; }
        public string? Description { get; set; }
        [ForeignKey(nameof(DeviceGroup))]
        public int? DeviceGroupId { get; set; }
        public DeviceGroup? DeviceGroup { get; set; }
        [ForeignKey(nameof(DeviceType))]
        public int? DeviceTypeId { get; set; }
        public DeviceType? DeviceType { get; set; }
        public string? DeviceModel { get; set; }
        public string? SerialNumber { get; set; }
        public string? Location { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; }

        private List<DeviceAddr>? _deviceAddresses;
        public virtual List<DeviceAddr> DeviceAddresses
        {
            get { return _deviceAddresses ?? (_deviceAddresses = new List<DeviceAddr>()); }
            set
            {
                _deviceAddresses = value ?? new List<DeviceAddr>();
            }
        }
        public Device()
        {
            Name = "New Device";
            DeviceAddresses = new List<DeviceAddr>();
        }

        public void AdjustData(bool validate)
        {
            if (DeviceTypeId == 0)
            {
                DeviceTypeId = null;
                DeviceType = null;
            }
            if (DeviceGroupId == 0)
            {
                DeviceGroupId = null;
                DeviceGroup = null;
            }
            if (DeviceAddresses?.Any() == true)
            {
                foreach (var i in DeviceAddresses)
                {
                    i.DeviceId = DeviceId;
                    i.AdjustData();
                }
            }
            if (validate)
            {
                //var context = new ValidationContext(this, serviceProvider: null, items: null);
                //var results = new List<ValidationResult>();
                //if (!Validator.TryValidateObject(this, context, results, true))
                //{
                //    throw new ValidationException("Device is not valid!");
                //}
                if (DeviceAddresses?.Any(x => string.IsNullOrEmpty(x.Addr)) == true) throw new ValidationException("Device Address is not valid!");
                if (DeviceAddresses?.Any(x => x.AddrTypeId <= 0) == true) throw new ValidationException("Device Address Type is not valid!");
            }
        }
        public Device Clone()
        {
            //Clone with system.text.json
            return System.Text.Json.JsonSerializer.Deserialize<Device>(System.Text.Json.JsonSerializer.Serialize(this)!)!;
        }
    }
}