using FBC.Devices.DBModels.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FBC.Devices.DBModels
{
    public class DeviceAddr: IHasPrimaryKey
    {
        [Key]
        public int DeviceAddrId { get; set; }
        public int PrimaryKeyId => DeviceAddrId;
        [ForeignKey(nameof(Device))]
        public int DeviceId { get; set; }
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
            Addr = "http://";
        }

        internal void AdjustData()
        {
            if (DeviceId == 0)
            {
                DeviceId = 0;
                //Device = null;
            }
            if (AddrTypeId == 0)
            {
                AddrTypeId = 0;
                AddrType = null;
            }
        }
    }
}