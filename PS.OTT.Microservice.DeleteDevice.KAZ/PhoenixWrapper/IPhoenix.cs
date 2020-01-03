using System.Threading.Tasks;

namespace PS.OTT.Microservice.DeleteDevice.KAZ.PhoenixWrapper
{
    public interface IPhoenix
    {
        Task<long?> GetDeviceFamilyIdAsync(long brandId);
        Task<bool> DeleteDeviceAsync(string udid);
    }
}