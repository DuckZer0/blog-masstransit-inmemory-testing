using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Log4NetIntegration;
using NUnit.Framework;

namespace MassTransitInMemoryTestingExample.Tests
{
    [TestFixture]
    public class PublishTest
    {
        private const string QueueName = "myQueue";
        private const string ErrorQueueName = "myQueue_error";
        private IBusControl _busControl;
        private Consumer<MyEvent> _fakeEventConsumer;
        private Consumer<Fault<MyEvent>> _fakeEventFaultConsumer;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private ConsumerRegistrar _consumerRegistrar;

        [SetUp]
        public void SetUp()
        {
            _fakeEventConsumer = new Consumer<MyEvent>(_manualResetEvent);
            _fakeEventFaultConsumer = new Consumer<Fault<MyEvent>>(_manualResetEvent);
            _consumerRegistrar = CreateSystemUnderTest();
            CreateBus();
            _busControl.Start();
        }

        private ConsumerRegistrar CreateSystemUnderTest()
        {
            return new ConsumerRegistrar(
                QueueName,
                new[] { typeof(Consumer<MyEvent>) },
                CreateConsumer());
        }

        private Func<Type, IConsumer> CreateConsumer()
        {
            // Very simple in this test as we've only got one type of consumer. In your production code, this func would probably use
            // an IoC container to resolve the consumer type.
            return consumerType => _fakeEventConsumer;
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
                    receiveEndpointConfigurator.Consumer(typeof(Consumer<Fault<MyEvent>>),
                        type => _fakeEventFaultConsumer);
                });
        }

        [Test]
        public async Task Registers_consumer_type_supplied_to_consumerRegistrar()
        {
            await PublishFakeEvent();
            WaitUntilBusHasProcessedMessageOrTimedOut(_manualResetEvent);

            Assert.That(_fakeEventConsumer.ReceivedMessage, Is.True);
            Assert.That(_fakeEventFaultConsumer.ReceivedMessage, Is.False);
        }

        private async Task PublishFakeEvent()
        {
            await _busControl.Publish(new MyEvent());
        }

        private void WaitUntilBusHasProcessedMessageOrTimedOut(ManualResetEvent manualResetEvent)
        {
            manualResetEvent.WaitOne(TimeSpan.FromSeconds(5));
        }
    }
}