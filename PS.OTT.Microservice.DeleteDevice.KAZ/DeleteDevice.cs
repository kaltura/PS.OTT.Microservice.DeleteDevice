using System;
using System.Net;
using System.Threading.Tasks;
using Kaltura;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PS.OTT.Core.MicroService.Infrastructure.Authentication;
using PS.OTT.Core.MicroService.Infrastructure.Exceptions;
using PS.OTT.Core.MicroService.Infrastructure.Swagger;
using PS.OTT.Microservice.KAZ.DeleteDevice.Models;
using PS.OTT.Microservice.KAZ.DeleteDevice.PhoenixWrapper;
using Swashbuckle.AspNetCore.Annotations;

namespace PS.OTT.Microservice.KAZ.DeleteDevice
{
    [KalturaAuthorize]
    [Route("api/p/{groupId:int}/service/[controller]/action/[action]")]
    public class DeleteDevice : Controller
    {
        private readonly ILogger<DeleteDevice> _logger;
        private readonly IPhoenix _phoenix;

        public DeleteDevice(ILogger<DeleteDevice> logger, IPhoenix phoenix)
        {
            _logger = logger;
            _phoenix = phoenix;
        }

        [HttpPost]
        [SwaggerImplementationNotes("Deletion of a household device except devices of an 'STB' type.")]
        public async Task<IActionResult> Delete([FromBody] DeleteDeviceRequest requestModel)
        {
            var deviceFamilyId = await _phoenix.GetDeviceFamilyIdAsync(requestModel.BrandId);
            if (deviceFamilyId == 2) // is stb device
                return BadRequest(new DeleteDeviceResponse { Message = "STB removal is forbidden. Please call the customer service." });

            await DeleteHouseholdDevice(requestModel.UDID);
            return Ok(new DeleteDeviceResponse { Message = "Device deleted successfully" });
        }

        private async Task DeleteHouseholdDevice(string udid)
        {
            const string defaultErrorCode = "9001";
            const string defaultErrorMessage = "Unexpected error while issuing a HouseholdDevice.Delete.";

            try
            {
                var deleteResult = await _phoenix.DeleteDeviceAsync(udid);
                if (deleteResult == false)
                {
                    _logger.LogError($"Kaltura core returned false while issuing a HouseholdDevice.Delete. UDID: {udid}");
                    throw new MicroserviceAPIException(defaultErrorCode, defaultErrorMessage);
                }
            }
            catch (Exception ex)
            {
                var apiException = (APIException) ex;
                var errorCode = !string.IsNullOrEmpty(apiException.Code) ? apiException.Code : defaultErrorCode;
                var errorMessage = !string.IsNullOrEmpty(apiException.Message) ? apiException.Message : defaultErrorMessage;
                _logger.LogError(ex, $"Error while deleting a household device. UDID: {udid}, Exception code: {errorCode}, " +
                              $"Exception message: {errorMessage}, Inner Exception: {ex.InnerException}.");
                throw new MicroserviceAPIException(errorCode, errorMessage);
            }
        }
    }
}