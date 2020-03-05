using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PS.OTT.Core.Logger;
using PS.OTT.Core.MicroService;
using PS.OTT.Core.MicroService.Infrastructure;
using PS.OTT.Core.MicroService.Infrastructure.Authentication;
using PS.OTT.Core.MicroService.Models.Exceptions;
using PS.OTT.Microservice.DeleteDevice.KAZ.PhoenixWrapper;

namespace PS.OTT.Microservice.DeleteDevice.KAZ
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MicroService.ServiceName = "Delete Device Microservice";
            MicroService.ServiceDescription = "Provides the ability to delete a device that's not of 'STB' type. " +
                                              "This page defines the micro-service that will be used to implement " +
                                              "this feature. Microservice specification: " +
            "https://kaltura.atlassian.net/wiki/spaces/VKE/pages/584254850/Micro-services#Micro-services-DeleteDeviceMicro-service";
            MicroService.ConfigureServicesAfterAddControllers = ConfigureServices;
            
            MicroService.Run(args);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetRequiredService<IMicroserviceLoggerFactory>();
            var startupLogger = loggerFactory.CreateLogger(nameof(Startup));

            services.AddMemoryCache();
            services.AddScoped<IPhoenix, Phoenix>();
            services.AddScoped(sp =>
            {
                try
                {
                    var clientFactory = sp.GetRequiredService<IKalturaClientFactory>();
                    var kalturaAuthenticationService = sp.GetRequiredService<IKalturaAuthenticationService>();
                    var kalturaClient = clientFactory.GetClient();

                    var ks = kalturaAuthenticationService.GetCurrentKalturaSession()?.Ks;
                    kalturaClient.setClientTag($"Microservice.{nameof(DeviceController)}");
                    kalturaClient.setKS(ks);
                    var logLength = kalturaClient.ResponseLogLength?.ToString() ?? "MAX";
                    startupLogger.Debug($"Kaltura client was created, with ResponseLogLength:[{logLength}], with KS:[{ks}].");
                    return kalturaClient;
                }
                catch (Exception ex)
                {
                    throw new MicroserviceAPIException("999500", "Cannot create Kaltura client.", ex);
                }
            });
        }
    }
}