using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PS.OTT.Core.Configuration.Repository;
using PS.OTT.Core.Configuration.Repository.Extensions;
using PS.OTT.Core.KalturaClientExtensions.ServiceExtensions;
using PS.OTT.Core.MicroService;
using PS.OTT.Core.MicroService.Infrastructure;
using PS.OTT.Core.MicroService.Infrastructure.Authentication;
using PS.OTT.Core.MicroService.Infrastructure.GroupId;
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
            MicroService.WrapResponseIntoResultWithExecTime = true;
            
            await MicroService.Run(args);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddOptions<AdapterConfigOptions>()
                .Configure<IGroupIdResolver>((opts, groupIdResolver) =>
                {
                    opts.KeyNamingStrategy = KeyNamingStrategy.Microservice;
                    opts.Key = groupIdResolver.Value.ToString();
                });
            services.AddCouchbaseConfig<Configuration>();

            services.AddOptions<KalturaUserClientOptions>().
               Configure<IConfigurationRepository<Configuration>>(
                    (opts, configurationRepo) =>
                    {
                        var configuration = configurationRepo.GetConfiguration();
                        opts.PhoenixApiVersion = configuration?.PhoenixApiVersion;
                        opts.ClientConfiguration = new Kaltura.Configuration
                        {
                            ServiceUrl = Settings.Instance.GetValue<string>("PhoenixUrl"),
                            Timeout = (configuration?.PhoenixTimeOutInSec ?? 30) * 1000
                        };
                    });

            services.AddKalturaClient();

            services.AddScoped<IPhoenix, Phoenix>();
        }
    }
}