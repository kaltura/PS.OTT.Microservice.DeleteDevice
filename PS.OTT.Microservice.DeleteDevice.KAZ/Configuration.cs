using PS.OTT.Core.MicroService.Infrastructure;

namespace PS.OTT.Microservice.DeleteDevice.KAZ
{
    [Configuration]
    public class Configuration
    {
        public string PhoenixApiVersion { get; set; }
        public short? PhoenixTimeOutInSec { get; set; }
    }
}