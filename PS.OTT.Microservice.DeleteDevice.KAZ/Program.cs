using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PS.OTT.Core.Configuration.Repository.Extensions;
using PS.OTT.Core.KalturaClientExtensions.ServiceExtensions;
using PS.OTT.Core.MicroService;
using PS.OTT.Core.MicroService.Infrastructure;
using PS.OTT.Core.MicroService.Infrastructure.Authentication;
using PS.OTT.Microservice.DeleteDevice.KAZ.PhoenixWrapper;
using TCMClient;

namespace PS.OTT.Microservice.DeleteDevice.KAZ
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            MicroService.ServiceName = "Delete Device Microservice";
            MicroService.ServiceDescription = "Provides the ability to delete a device that's not of 'STB' type. " +
                                              "This page defines the micro-service that will be used to implement " +
                                              "this feature. Microservice specification: " +
            "https://kaltura.atlassian.net/wiki/spaces/VKE/pages/584254850/Micro-services#Micro-services-DeleteDeviceMicro-service";
            MicroService.ConfigureServicesAfterAddControllers = ConfigureServices;
            
            await MicroService.Run(args);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddOptions<AdapterConfigOptions>()
                .Configure<IGroupIdResolver>((opts, groupIdResolver) =>
                {
                    opts.KeyNamingStrategy = KeyNamingStrategy.Microservice;
                    opts.Key = groupIdResolver.GetGroupId().ToString();
                });
            services.AddCouchbaseConfig<Configuration>();

            services.AddOptions<KalturaUserClientOptions>()
                .Configure<Configuration, IKalturaAuthenticationService>(
                    (opts, config, authService) =>
                    {
                        opts.PhoenixApiVersion = config?.PhoenixApiVersion;
                        opts.Ks = authService.GetCurrentKalturaSession().Ks;
                        opts.ClientConfiguration = new Kaltura.Configuration
                        {
                            ServiceUrl = Settings.Instance.GetValue<string>("PhoenixUrl"),
                            Timeout = (config?.PhoenixTimeOutInSec ?? 30) * 1000
                        };
                    });

            services.AddKalturaUserClient();

            services.AddScoped<IPhoenix, Phoenix>();
        }
    }
}