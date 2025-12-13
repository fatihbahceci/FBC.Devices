using FBC.Devices.API.DBModels.Repository;
using FBC.DBRepository;
using System.ComponentModel.DataAnnotations;

namespace FBC.Devices.DBModels
{
    /// <summary>
    /// HTTP, RTSP, FTP, SSH, Telnet, etc.
    /// </summary>
    public class AddrType : EntityBase<AddrType>
    {
        public string Name { get; set; }
        public AddrType()
        {
            Name = "New Address Type";
        }

        public override void CheckDataFor(EntityOperation operation, bool alsoValidate, IQueryable<AddrType> query)
        {
            if (alsoValidate)
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    throw new ValidationException("AddrType Name cannot be empty.");
                }
            }

        }
    }
}