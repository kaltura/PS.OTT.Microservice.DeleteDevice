using System;
using System.Net;
using System.Threading.Tasks;
using Kaltura;
using Microsoft.AspNetCore.Mvc;
using PS.OTT.Core.Logger;
using PS.OTT.Core.MicroService.Controllers.Common;
using PS.OTT.Core.MicroService.Infrastructure.Authentication;
using PS.OTT.Core.MicroService.Infrastructure.Swagger;
using PS.OTT.Core.MicroService.Models.Common;
using PS.OTT.Core.MicroService.Models.Exceptions;
using PS.OTT.Microservice.DeleteDevice.KAZ.Models;
using PS.OTT.Microservice.DeleteDevice.KAZ.PhoenixWrapper;
using Swashbuckle.AspNetCore.Annotations;

namespace PS.OTT.Microservice.DeleteDevice.KAZ
{
	[KalturaAuthorize]
	[Route("api/p/{groupId:int}/service/[controller]/action/[action]")]
	public class DeviceController : BaseApiController
	{
		private readonly IMicroServiceLogger _logger;
		private readonly IPhoenix _phoenix;

        public DeviceController(IMicroserviceLoggerFactory loggerFactory, IPhoenix phoenix)
        {
			_logger = loggerFactory.CreateLogger(nameof(DeviceController));
            _phoenix = phoenix;
        }

		[HttpPost]
		[SwaggerImplementationNotes("Deletion of a household device except devices of an 'STB' type.")]
		[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(CommonApiResponse<DeleteDeviceResponse>))]
		[SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(CommonApiResponse<DeleteDeviceResponse>), 
            Description = "Can't remove device of type STB.")]
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
					_logger.Error($"Kaltura core returned false while issuing a HouseholdDevice.Delete. UDID: {udid}");
					throw new MicroserviceAPIException(defaultErrorCode, defaultErrorMessage);
				}
			}
			catch (Exception ex)
			{
				var apiException = (APIException) ex;
				var errorCode = !string.IsNullOrEmpty(apiException.Code) ? apiException.Code : defaultErrorCode;
				var errorMessage = !string.IsNullOrEmpty(apiException.Message) ? apiException.Message : defaultErrorMessage;
				_logger.Error($"Error while deleting a household device. UDID: {udid}, Exception code: {errorCode}, " +
                              $"Exception message: {errorMessage}, Inner Exception: {ex.InnerException}.");
				throw new MicroserviceAPIException(errorCode, errorMessage);
			}
        }
	}
}