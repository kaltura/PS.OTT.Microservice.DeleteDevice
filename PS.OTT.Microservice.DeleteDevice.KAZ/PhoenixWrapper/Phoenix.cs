using System.Linq;
using System.Threading.Tasks;
using Kaltura;
using Kaltura.Services;

namespace PS.OTT.Microservice.DeleteDevice.KAZ.PhoenixWrapper
{
    public class Phoenix : IPhoenix
    {
        private readonly Client _kalturaClient;

        public Phoenix(Client kalturaClient) => _kalturaClient = kalturaClient;

        public async Task<long?> GetDeviceFamilyIdAsync(long brandId)
        {
            var deviceBrands = await DeviceBrandService.List().ExecuteAsync(_kalturaClient);
            var deviceFamilyID = deviceBrands.Objects.FirstOrDefault(deviceBrand => deviceBrand.Id == brandId)?.DeviceFamilyid;
            return deviceFamilyID;
        }

        public async Task<bool> DeleteDeviceAsync(string udid) => await HouseholdDeviceService.Delete(udid).ExecuteAsync(_kalturaClient);
    }
}