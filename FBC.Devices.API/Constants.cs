using System.Security.Cryptography;
using System.Text;

namespace FBC.Devices.API;

public static class Constants
{
    public static class UserRoles
    {
        public const string SysAdmin = "SysAdmin";
        public const string ViewDevices = "ViewDevices";
        public const string EditDevices = "Edit.Devices";
        public const string EditDeviceGroups = "Edit.DeviceGroups";
        public const string EditDeviceTypes = "Edit.DeviceTypes";
        public const string EditDeviceAddrTypes = "Edit.DeviceAddrTypes";

        public static readonly List<string> AllRolesButSysAdmin = new()
        {
            ViewDevices,
            EditDevices,
            EditDeviceGroups,
            EditDeviceTypes,
            EditDeviceAddrTypes
        };
    }

    public static class Tools
    {
        public static string ToMD5(string str)
        {
            var inputBytes = Encoding.ASCII.GetBytes(str);
            var hashBytes = MD5.HashData(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
