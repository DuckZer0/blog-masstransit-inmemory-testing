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
        private Consumer<MyCommand> _myCommandConsumer;
        private Consumer<Fault<MyCommand>> _myCommandFaultConsumer;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        [SetUp]
        public void SetUp()
        {
            _myCommandConsumer = new Consumer<MyCommand>(_manualResetEvent);
            _myCommandFaultConsumer = new Consumer<Fault<MyCommand>>(_manualResetEvent);
            CreateBus();
            _busControl.Start();
        }

        private void CreateBus()
        {
            _busControl = Bus.Factory.CreateUsingInMemory(ConfigureBus);
        }

        private void ConfigureBus(IBusFactoryConfigurator busFactoryConfigurator)
        {
            busFactoryConfigurator.UseLog4Net();
            ConfigureReceiveEndpoints(busFactoryConfigurator);
        }

        private void ConfigureReceiveEndpoints(IBusFactoryConfigurator busFactoryConfigurator)
        {
            ConfigureReceiveEndpointForMyCommand(busFactoryConfigurator);
            ConfigureReceiveEndpointToListenForFaults(busFactoryConfigurator);
        }

        private void ConfigureReceiveEndpointForMyCommand(IBusFactoryConfigurator busFactoryConfigurator)
        {
            busFactoryConfigurator.ReceiveEndpoint(QueueName, receiveEndpointConfigurator =>
            {
                receiveEndpointConfigurator.Consumer(typeof(Consumer<MyCommand>), consumerType => _myCommandConsumer);
            });
        }

        private void ConfigureReceiveEndpointToListenForFaults(IBusFactoryConfigurator busFactoryConfigurator)
        {
            busFactoryConfigurator.ReceiveEndpoint(ErrorQueueName, receiveEndpointConfigurator =>
            {
                receiveEndpointConfigurator.Consumer(typeof(Consumer<Fault<MyCommand>>), type => _myCommandFaultConsumer);
            });
        }

        [Test]
        public async Task Registers_consumer_type_supplied_to_consumerRegistrar()
        {
            await SendMyCommand();
            WaitUntilBusHasProcessedMessageOrTimedOut(_manualResetEvent);

            Assert.That(_myCommandConsumer.ReceivedMessage, Is.True);
            Assert.That(_myCommandFaultConsumer.ReceivedMessage, Is.False);
        }

        private async Task SendMyCommand()
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
