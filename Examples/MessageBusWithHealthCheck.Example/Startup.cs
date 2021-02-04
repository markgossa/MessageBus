using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Microsoft.ServiceBus;
using MessageBusWithHealthCheck.Example.Events;
using MessageBusWithHealthCheck.Example.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBusWithHealthCheck.Example
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
            services.SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>()
                .AddMessageBus(new AzureServiceBusClientBuilder(Configuration["ServiceBus:Hostname"],
                    Configuration["ServiceBus:Topic"], Configuration["ServiceBus:Subscription"],
                    Configuration["ServiceBus:TenantId"]))
                .AddHostedService<MessageBusHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }
}
