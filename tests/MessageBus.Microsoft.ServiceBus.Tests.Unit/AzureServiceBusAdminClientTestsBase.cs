namespace MessageBus.Microsoft.ServiceBus.Tests.Unit
{
    public class AzureServiceBusAdminClientTestsBase
    {
        protected const string _connectionString = "Endpoint=sb://testsb.servicebus.windows.net/;" +
            "SharedAccessKeyName=Manage;SharedAccessKey=testsharedaccesskey;";
        protected const string _topic = "topic1";
        protected const string _subscription = "subscription1";
        protected const string _hostname = "test.servicebus.windows.net";
        protected const string _tenantId = "12345-12345-12345-12345-12345-12345";
    }
}