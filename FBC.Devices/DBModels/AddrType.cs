using System.ComponentModel.DataAnnotations;

namespace FBC.Devices.DBModels
{
    /// <summary>
    /// HTTP, RTSP, FTP, SSH, Telnet, etc.
    /// </summary>
    public class AddrType
    {
        [Key]
        public int AddrTypeId { get; set; }
        public string Name { get; set; }
        public AddrType()
        {
            Name = "New Address Type";
        }
    }
}