using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Microsoft.ServiceBus;
using MessageBus.HostedService.Example.Events;
using MessageBus.HostedService.Example.Handlers;
using MessageBus.HostedService.Example.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBus.HostedService.Example
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<MessageBusHostedService>()
                .AddSingleton<IDependency, SomeDependency>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["ServiceBus:Hostname"],
                        Configuration["ServiceBus:Topic"], Configuration["ServiceBus:Subscription"],
                        Configuration["ServiceBus:TenantId"]))
                    .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();
            services.AddHealthChecks().AddCheck<MessageBusHealthCheck>("MessageBus");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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