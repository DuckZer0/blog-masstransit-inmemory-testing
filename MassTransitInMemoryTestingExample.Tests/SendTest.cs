using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net.Config;
using MassTransit;
using MassTransit.Log4NetIntegration;
using NUnit.Framework;

namespace MassTransitInMemoryTestingExample.Tests
{
    [TestFixture]
    public class SendTest
    {
        private const string QueueName = "myQueue";
        private const string ErrorQueueName = "myQueue_error";
        private const string LoopbackAddress = "loopback://localhost/";
        private IBusControl _busControl;
        private Consumer<MyCommand> _fakeCommandConsumer;
        private Consumer<Fault<MyCommand>> _fakeCommandFaultConsumer;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private ConsumerRegistrar _consumerRegistrar;

        [SetUp]
        public void SetUp()
        {
            _fakeCommandConsumer = new Consumer<MyCommand>(_manualResetEvent);
            _fakeCommandFaultConsumer = new Consumer<Fault<MyCommand>>(_manualResetEvent);
            _consumerRegistrar = CreateSystemUnderTest();
            ConfigureLog4Net();
            CreateBus();
            _busControl.Start();
        }

        private ConsumerRegistrar CreateSystemUnderTest()
        {
            return new ConsumerRegistrar(
                QueueName,
                new[] { typeof(Consumer<MyCommand>) },
                CreateConsumer());
        }

        private Func<Type, IConsumer> CreateConsumer()
        {
            // Very simple in this test as we've only got one type of consumer. In your production code, this func would probably use
            // an IoC container to resolve the consumer type.
            return consumerType => _fakeCommandConsumer;
        }

        private void ConfigureLog4Net()
        {
            XmlConfigurator.Configure(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
        }

        private void CreateBus()
        {
            _busControl = Bus.Factory.CreateUsingInMemory(ConfigureBus);
        }

        private void ConfigureBus(IInMemoryBusFactoryConfigurator inMemoryBusFactoryConfigurator)
        {
            inMemoryBusFactoryConfigurator.UseLog4Net();
            ConfigureReceiveEndpoints(inMemoryBusFactoryConfigurator);
        }

        private void ConfigureReceiveEndpoints(IInMemoryBusFactoryConfigurator inMemoryBusFactoryConfigurator)
        {
            inMemoryBusFactoryConfigurator.ReceiveEndpoint(_consumerRegistrar.QueueName, _consumerRegistrar.ConfigureEndpoint);

            ConfigureReceiveEndpointToListenForFaults(inMemoryBusFactoryConfigurator);
        }

        private void ConfigureReceiveEndpointToListenForFaults(IInMemoryBusFactoryConfigurator inMemoryBusFactoryConfigurator)
        {
            inMemoryBusFactoryConfigurator.ReceiveEndpoint(ErrorQueueName,
                receiveEndpointConfigurator =>
                {
                    receiveEndpointConfigurator.Consumer(typeof(Consumer<Fault<MyCommand>>),
                        type => _fakeCommandFaultConsumer);
                });
        }

        [Test]
        public async Task Registers_all_consumers_listed_in_consumerTypeProvider_with_the_queue_from_the_receiveEndpointConfiguration()
        {
            await SendFakeCommand();
            WaitUntilBusHasProcessedMessageOrTimedOut(_manualResetEvent);

            Assert.That(_fakeCommandConsumer.ReceivedMessage, Is.True);
            Assert.That(_fakeCommandFaultConsumer.ReceivedMessage, Is.False);
        }

        private async Task SendFakeCommand()
        {
            var sendEndpoint = await GetSendEndpoint();
            await sendEndpoint.Send(new MyCommand());
        }

        private async Task<ISendEndpoint> GetSendEndpoint()
        {
            return await _busControl.GetSendEndpoint(new Uri($"{LoopbackAddress}{QueueName}"));
        }

        private void WaitUntilBusHasProcessedMessageOrTimedOut(ManualResetEvent manualResetEvent)
        {
            manualResetEvent.WaitOne(TimeSpan.FromSeconds(5));
        }
    }
}
