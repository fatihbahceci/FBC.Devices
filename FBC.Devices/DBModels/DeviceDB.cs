using FBC.Devices.DBModels.Helpers;

namespace FBC.Devices.DBModels
{
    public class DeviceDB(DB db)
    {
        private DB db = db;

        private AddrTypeHelper? addrTypeHelper;
        private DeviceGroupHelper? deviceGroupHelper;
        private DeviceTypeHelper? deviceTypeHelper;
        private DeviceHelper? deviceHelper;
        private DeviceAddrHelper? deviceAddrHelper; 
        public AddrTypeHelper AddrTypes { get => addrTypeHelper ??= new AddrTypeHelper(this, db); }
        public DeviceGroupHelper DeviceGroups { get => deviceGroupHelper ??= new DeviceGroupHelper(this, db); }
        public DeviceTypeHelper DeviceTypes { get => deviceTypeHelper ??= new DeviceTypeHelper(this, db); }
        public DeviceHelper Devices { get => deviceHelper ??= new DeviceHelper(this, db); }
        public DeviceAddrHelper DeviceAddresses { get => deviceAddrHelper ??= new DeviceAddrHelper(this, db); }
        public DB DB { get => db; }
    }
}
