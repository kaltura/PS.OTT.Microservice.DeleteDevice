using System.Threading.Tasks;

namespace PS.OTT.Microservice.KAZ.DeleteDevice.PhoenixWrapper
{
    public interface IPhoenix
    {
        Task<long?> GetDeviceFamilyIdAsync(long brandId);
        Task<bool> DeleteDeviceAsync(string udid);
    }
}