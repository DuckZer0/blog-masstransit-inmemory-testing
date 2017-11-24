using System;
using System.Threading;
using System.Threading.Tasks;
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

        [SetUp]
        public void SetUp()
        {
            _fakeCommandConsumer = new Consumer<MyCommand>(_manualResetEvent);
            _fakeCommandFaultConsumer = new Consumer<Fault<MyCommand>>(_manualResetEvent);
            CreateBus();
            _busControl.Start();
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
            ConfigureReceiveEndpointForMyCommand(inMemoryBusFactoryConfigurator);
            ConfigureReceiveEndpointToListenForFaults(inMemoryBusFactoryConfigurator);
        }

        private void ConfigureReceiveEndpointForMyCommand(IInMemoryBusFactoryConfigurator inMemoryBusFactoryConfigurator)
        {
            inMemoryBusFactoryConfigurator.ReceiveEndpoint(QueueName, receiveEndpointConfigurator =>
            {
                receiveEndpointConfigurator.Consumer(typeof(Consumer<MyCommand>), consumerType => _fakeCommandConsumer);
            });
        }

        private void ConfigureReceiveEndpointToListenForFaults(IInMemoryBusFactoryConfigurator inMemoryBusFactoryConfigurator)
        {
            inMemoryBusFactoryConfigurator.ReceiveEndpoint(ErrorQueueName, receiveEndpointConfigurator =>
            {
                receiveEndpointConfigurator.Consumer(typeof(Consumer<Fault<MyCommand>>), type => _fakeCommandFaultConsumer);
            });
        }

        [Test]
        public async Task Registers_consumer_type_supplied_to_consumerRegistrar()
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
