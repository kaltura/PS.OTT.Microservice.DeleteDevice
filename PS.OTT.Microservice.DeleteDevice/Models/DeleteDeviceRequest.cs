namespace PS.OTT.Microservice.DeleteDevice.Models
{
    public class DeleteDeviceRequest
    {
        public string UDID { get; set; }
        public int BrandId { get; set; }
    }
}