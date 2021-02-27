using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xunit;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit
{
    public class MessageProcessorResolverTests
    {
        [Fact]
        public void AddsAndRetrievesMessagePreProcessors()
        {
            var services = new ServiceCollection();
            
            var sut = new MessageProcessorResolver(services);
            sut.AddMessagePreProcessor<TestPreProcessor1>();
            sut.AddMessagePreProcessor<TestPreProcessor2>();
            sut.Initialize();
            var preProcessors = sut.GetMessagePreProcessors();

            Assert.Equal(2, preProcessors.Count());
            Assert.NotNull(preProcessors.First());
            Assert.IsType<TestPreProcessor1>(preProcessors.First());
            Assert.NotNull(preProcessors.Last());
            Assert.IsType<TestPreProcessor2>(preProcessors.Last());
        }
        
        [Fact]
        public void AddsAndRetrievesMessagePostProcessors()
        {
            var services = new ServiceCollection();
            
            var sut = new MessageProcessorResolver(services);
            sut.AddMessagePostProcessor<TestPostProcessor1>();
            sut.AddMessagePostProcessor<TestPostProcessor2>();
            sut.Initialize();
            var postProcessors = sut.GetMessagePostProcessors();

            Assert.Equal(2, postProcessors.Count());
            Assert.NotNull(postProcessors.First());
            Assert.IsType<TestPostProcessor1>(postProcessors.First());
        }
        
        [Fact]
        public void AddsAndRetrievesMessageProcessors()
        {
            var services = new ServiceCollection();
            
            var sut = new MessageProcessorResolver(services);
            sut.AddMessagePreProcessor<TestPreProcessor1>();
            sut.AddMessagePostProcessor<TestPostProcessor1>();
            sut.Initialize();
            var preProcessors = sut.GetMessagePreProcessors();
            var postProcessors = sut.GetMessagePostProcessors();

            Assert.Single(preProcessors);
            Assert.Single(postProcessors);
            Assert.NotNull(preProcessors.First());
            Assert.IsType<TestPreProcessor1>(preProcessors.First());
            Assert.NotNull(postProcessors.First());
            Assert.IsType<TestPostProcessor1>(postProcessors.First());
        }
        
        [Fact]
        public void ReturnsEmptyIEnumerableIfNoProcessorsAdded()
        {
            var sut = new MessageProcessorResolver(new ServiceCollection());
            
            sut.Initialize();

            Assert.Empty(sut.GetMessagePreProcessors());
            Assert.Empty(sut.GetMessagePostProcessors());
        }
    }
}
