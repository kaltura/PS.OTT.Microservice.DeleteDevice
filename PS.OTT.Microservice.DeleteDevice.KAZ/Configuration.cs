using PS.OTT.Core.MicroService.Infrastructure.Configuration;

namespace PS.OTT.Microservice.KAZ.DeleteDevice
{
    [Configuration]
    public class Configuration
    {
        public string PhoenixApiVersion { get; set; }
        public short? PhoenixTimeOutInSec { get; set; }
    }
}