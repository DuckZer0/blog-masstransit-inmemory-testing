using System;
using System.Linq;
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
        private MyEventConsumer _myEventConsumer;
        private MyEventFaultConsumer _myEventFaultConsumer;

        [SetUp]
        public void SetUp()
        {
            _myEventConsumer = new MyEventConsumer();
            _myEventFaultConsumer = new MyEventFaultConsumer();
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
            ConfigureReceiveEndpointsToListenForMyEvent(busFactoryConfigurator);
            ConfigureReceiveEndpointToListenForFaults(busFactoryConfigurator);
        }

        private void ConfigureReceiveEndpointsToListenForMyEvent(IBusFactoryConfigurator busFactoryConfigurator)
        {
            busFactoryConfigurator.ReceiveEndpoint(QueueName,
                receiveEndpointConfigurator =>
                {
                    receiveEndpointConfigurator.Consumer(typeof(MyEventConsumer), consumerType => _myEventConsumer);
                });
        }

        private void ConfigureReceiveEndpointToListenForFaults(IBusFactoryConfigurator busFactoryConfigurator)
        {
            busFactoryConfigurator.ReceiveEndpoint(ErrorQueueName,
                receiveEndpointConfigurator =>
                {
                    receiveEndpointConfigurator.Consumer(typeof(MyEventFaultConsumer), type => _myEventFaultConsumer);
                });
        }

        [Test]
        public async Task Consumer_has_been_registered_to_receive_message()
        {
            await PublishMyEvent();
            WaitUntilConditionMetOrTimedOut(() => State.EventsReceived.Any());

            Assert.That(State.EventsReceived.Count, Is.EqualTo(1));
            Assert.That(State.EventFaultsReceived.Count, Is.EqualTo(0));
        }

        private async Task PublishMyEvent()
        {
            await _busControl.Publish(new MyEvent());
        }

        private void WaitUntilConditionMetOrTimedOut(Func<bool> conditionMet)
        {
            var timeoutExpired = false;
            var startTime = DateTime.Now;
            while (!conditionMet() && !timeoutExpired)
            {
                Thread.Sleep(100);
                timeoutExpired = DateTime.Now - startTime > TimeSpan.FromSeconds(5);
            }
        }
    }
}