using System.Threading.Tasks;

namespace MessageBusWithHealthCheck.Example.Services
{
    public interface IDependency
    {
        Task WriteLineAfterDelay(string text);
    }
}