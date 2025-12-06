using FBC.Devices.API.DBModels.Repository;
using System.ComponentModel.DataAnnotations.Schema;

namespace FBC.Devices.DBModels
{
    public class DeviceAddr : Entity<long>
    {
        [ForeignKey(nameof(Device))]
        public long DeviceId { get; set; }
        //public Device? Device { get; set; }
        [ForeignKey(nameof(AddrType))]
        public int AddrTypeId { get; set; }
        public AddrType? AddrType { get; set; }
        public string? Addr { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool PeriodicPingCheck { get; set; }
        public DeviceAddr()
        {
            Addr = string.Empty;
        }

        public override void CheckDataFor(EntityOperation entityOperation, bool alsoValidate)
        { if (DeviceId == 0)
            {
                DeviceId = 0;
                //Device = null;
            }
            if (AddrTypeId == 0)
            {
                AddrTypeId = 0;
                AddrType = null;
            }
            if (alsoValidate)
            {
                if (string.IsNullOrWhiteSpace(Addr))
                {
                    throw new ArgumentException("Addr is required");
                }
            }
        }
    }
}