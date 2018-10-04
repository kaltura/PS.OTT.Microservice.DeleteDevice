using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Kaltura;
using Kaltura.Enums;
using Kaltura.Request;
using Kaltura.Services;
using Kaltura.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PS.OTT.Core.MicroService.Controllers.Common;
using PS.OTT.Core.MicroService.Infrastructure;
using PS.OTT.Core.MicroService.Infrastructure.Authentication;
using PS.OTT.Core.MicroService.Infrastructure.Logging;
using PS.OTT.Core.MicroService.Infrastructure.Swagger;
using PS.OTT.Core.MicroService.Models.Common;
using PS.OTT.Core.MicroService.Models.Exceptions;
using PS.OTT.Microservice.DeleteDevice.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PS.OTT.Microservice.DeleteDevice
{
	[KalturaAuthorize()]
	[Route("api/p/{groupId:int}/service/[controller]/action/[action]")]
	public class DeviceController : BaseApiController
	{
		private readonly IMicroServiceLogger _Logger;
		private readonly Client _KalturaClient;
		private int _GroupId => int.Parse((string)RouteData.Values["groupId"]);

		public DeviceController(
			IMicroserviceLoggerFactory loggerFactory,
			IKalturaClientFactory kalturaClientFactory,
			IKalturaAuthenticationService kalturaAuthenticationService): base()
		{
			_Logger = loggerFactory.CreateLogger(nameof(DeviceController));
			var kalturaSession = kalturaAuthenticationService.GetCurrentKalturaSession();
			_KalturaClient = kalturaClientFactory.GetClient();
			_KalturaClient.setKS(kalturaSession.Ks);
		}

		[HttpPost()]
		[SwaggerImplementationNotes("Deletion of a household device except devices of an 'STB' type.")]
		[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(CommonApiResponse<DeleteDeviceResponse>))]
		[SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(CommonApiResponse<DeleteDeviceResponse>), Description = "Can't remove device of type STB.")]
		public IActionResult Delete([FromBody] DeleteDeviceRequest requestModel)
		{
			_Logger.Info($"Got request with values: UDID - [{requestModel.UDID}], BrandId - [{requestModel.BrandId}]");
			var isSTB = IsBrandIdOfSetTopBoxType(requestModel.BrandId);
			if(isSTB) { return BadRequest(new DeleteDeviceResponse { Message = "STB removal is forbidden. Please call the customer service" }); }
			DeleteHouseholdDevice(requestModel.UDID);
			return Ok(new DeleteDeviceResponse { Message = "Device deleted successfully" });
		}

        private bool IsBrandIdOfSetTopBoxType(int brandId)
        {
            var deviceBrands = DeviceBrandService.List().ExecuteAndWaitForResponse(_KalturaClient);
            var deviceFamilyID = deviceBrands.Objects.FirstOrDefault(deviceBrand => deviceBrand.Id == brandId)?.DeviceFamilyid;
            return deviceFamilyID == 2;
        }

        private void DeleteHouseholdDevice(string udid)
        {
			var defaultErrorCode = "9001";
			var defaultErrorMessage = "Unexpected error while issuing a HouseholdDevice.Delete";

			try
			{
				var deleteDeviceRequest = HouseholdDeviceService.Delete(udid);
				var response = deleteDeviceRequest.ExecuteAndWaitForResponse(_KalturaClient);
				if(response == false)
				{
					_Logger.Error($"Kaltura core returned false while issuing a HouseholdDevice.Delete. UDID: {udid}");
					throw new MicroserviceAPIException(defaultErrorCode, defaultErrorMessage);
				}
			}
			catch (Exception ex)
			{
				var apiException = (APIException) ex;
				var errorCode = (apiException != null && !string.IsNullOrEmpty(apiException.Code)) ? apiException.Code : defaultErrorCode;
				var errorMessage = (apiException != null && !string.IsNullOrEmpty(apiException.Message)) ? apiException.Message : defaultErrorMessage;
				_Logger.Error($"Error while deleting a household device. UDID: {udid}, Exception code: {errorCode}, Exception message: {errorMessage}, Inner Exception: {ex.InnerException}");
				throw new MicroserviceAPIException(errorCode, errorMessage);
			}
        }
	}
}