using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Microsoft.ServiceBus;
using MessageBus.HostedService.Example.Events;
using MessageBus.HostedService.Example.Handlers;
using MessageBus.HostedService.Example.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MessageBus.HostedService.Example.Processors;

namespace MessageBus.HostedService.Example
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IDependency, SomeDependency>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["ServiceBus:Hostname"],
                        Configuration["ServiceBus:Topic"], Configuration["ServiceBus:Subscription"],
                        Configuration["ServiceBus:TenantId"]))
                    .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>()
                    .AddMessagePreProcessor<MessageReceivedLogger>()
                    .AddMessagePostProcessor<MessageProcessedLogger>();
            services.AddHealthChecks().AddCheck<MessageBusHealthCheck>("MessageBus");
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
