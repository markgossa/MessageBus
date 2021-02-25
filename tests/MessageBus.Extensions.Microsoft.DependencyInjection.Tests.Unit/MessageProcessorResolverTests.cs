using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit
{
    public class MessageProcessorResolverTests
    {
        [Fact]
        public void RetrievesMessagePreProcessor()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IMessagePreProcessor>(new TestPreProcessor1());
            services.AddSingleton<IMessagePreProcessor>(new TestPreProcessor2());

            var sut = new MessageProcessorResolver(services);
            sut.Initialize();
            var preProcessor1 = sut.Resolve<TestPreProcessor1>();
            var preProcessor2 = sut.Resolve<TestPreProcessor2>();

            Assert.NotNull(preProcessor1);
            Assert.IsType<TestPreProcessor1>(preProcessor1);
            Assert.NotNull(preProcessor2);
            Assert.IsType<TestPreProcessor2>(preProcessor2);
        }
        
        [Fact]
        public void RetrievesMessagePostProcessor()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IMessagePostProcessor>(new TestPostProcessor1());
            services.AddSingleton<IMessagePostProcessor>(new TestPostProcessor2());

            var sut = new MessageProcessorResolver(services);
            sut.Initialize();
            var postProcessor1 = sut.Resolve<TestPostProcessor1>();
            var postProcessor2 = sut.Resolve<TestPostProcessor2>();

            Assert.NotNull(postProcessor1);
            Assert.IsType<TestPostProcessor1>(postProcessor1);
            Assert.NotNull(postProcessor2);
            Assert.IsType<TestPostProcessor2>(postProcessor2);
        }
        
        [Fact]
        public void ThrowsMessageProcessorNotFoundExceptionIfMessageProcessorNotFound()
        {
            var services = new ServiceCollection();

            var sut = new MessageProcessorResolver(services);
            sut.Initialize();
            object testMethod() => sut.Resolve<TestPreProcessor1>();

            var ex = Assert.Throws<MessageProcessorNotFound>(testMethod);
            Assert.Contains(nameof(TestPreProcessor1), ex.Message);
        }
    }
}
