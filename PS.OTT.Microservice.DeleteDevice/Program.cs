using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PS.OTT.Core.MicroService;

namespace PS.OTT.Microservice.DeleteDevice
{
	public class Program
	{
		public static void Main (string[] args)
		{
			PS.OTT.Core.MicroService.MicroService.ServiceName = "Delete Device Microservice";
			PS.OTT.Core.MicroService.MicroService.ServiceDescription = "Provides the ability to delete a device that's not of 'STB' type. This page defines the micro-service that will be used to implement this feature. Microservice specification: https://kaltura.atlassian.net/wiki/spaces/VKE/pages/584254850/Micro-services#Micro-services-DeleteDevicemicro-service";
			PS.OTT.Core.MicroService.MicroService.ServiceVersion = "v1";
			PS.OTT.Core.MicroService.MicroService.ConfigureServices = ConfigureServices;
			PS.OTT.Core.MicroService.MicroService.ConfigureApplication = ConfigureApplication;
			PS.OTT.Core.MicroService.MicroService.IsConfigurationRequired = false;
			PS.OTT.Core.MicroService.MicroService.Run(args);
		}

		private static void ConfigureApplication(IApplicationBuilder app, IHostingEnvironment env)
		{
		}

		private static void ConfigureServices(IServiceCollection services)
		{
		}
	}
}