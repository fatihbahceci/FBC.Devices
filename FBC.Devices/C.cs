namespace FBC.Devices
{
    public class C
    {
        public static class DBQ
        {
            public static class Ex
            {
                public const string Include = "Include";
                public const string UserRights = "UserRights";
            }
        }
        public static class UserRoles
        {
            //public static List<string> AllRoles = new List<string>
            //{
            //    SysAdmin,
            //    ViewDevices,
            //    EditDevices,
            //    EditDeviceGroups,
            //    EditDeviceTypes,
            //    EditDeviceAddrTypes
            //};
            public const string SysAdmin = "SysAdmin";
            public const string ViewDevices = "ViewDevices";
            public const string EditDevices = "Edit.Devices";
            public const string EditDeviceGroups = "Edit.DeviceGroups";
            public const string EditDeviceTypes = "Edit.DeviceTypes";
            public const string EditDeviceAddrTypes = "Edit.DeviceAddrTypes";


        }

        public static class Tools
        {
            public static string ToMD5(string str)
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    var inputBytes = System.Text.Encoding.ASCII.GetBytes(str);
                    var hashBytes = md5.ComputeHash(inputBytes);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
            public static string ToMD5(byte[] bytes)
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    var hashBytes = md5.ComputeHash(bytes);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        public static class NAV
        {
            public const string HomePage = "/";
            public const string Login = "/Login";
            public static class Params
            {
                public const string Redirect = "Redirect";
                //public const string DeviceId = "DeviceId";
                //public const string DeviceGroupId = "DeviceGroupId";
                //public const string DeviceTypeId = "DeviceTypeId";
                //public const string AddrTypeId = "AddrTypeId";
                //public const string DeviceAddrId = "DeviceAddrId";
            }
            public static class Edit
            {
                public const string DeviceList = "/Edit/DeviceList";
                public const string DeviceGroupList = "/Edit/DeviceGroupList";
                public const string DeviceTypeList = "/Edit/DeviceTypeList";
                public const string AddrTypeList = "/Edit/AddrTypeList";
                public const string EditDevice = "/Edit/EditDevice/";
                //public const string DeviceAddrList = "/Edit/DeviceAddrList";
            }
        }
    }
}
