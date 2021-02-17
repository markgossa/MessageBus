using Microsoft.Extensions.Configuration;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration.Models
{
    public class Settings
    {
        public IConfiguration Configuration { get; }

        public Settings()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddUserSecrets<Settings>()
                .Build();
        }
    }
}
