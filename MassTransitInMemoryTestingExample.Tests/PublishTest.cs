﻿using System;
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
        private Consumer<MyEvent> _myEventConsumer;
        private Consumer<Fault<MyEvent>> _myEventFaultConsumer;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        [SetUp]
        public void SetUp()
        {
            _myEventConsumer = new Consumer<MyEvent>(_manualResetEvent);
            _myEventFaultConsumer = new Consumer<Fault<MyEvent>>(_manualResetEvent);
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
                    receiveEndpointConfigurator.Consumer(typeof(Consumer<MyEvent>), consumerType => _myEventConsumer);
                });
        }

        private void ConfigureReceiveEndpointToListenForFaults(IBusFactoryConfigurator busFactoryConfigurator)
        {
            busFactoryConfigurator.ReceiveEndpoint(ErrorQueueName,
                receiveEndpointConfigurator =>
                {
                    receiveEndpointConfigurator.Consumer(typeof(Consumer<Fault<MyEvent>>), type => _myEventFaultConsumer);
                });
        }

        [Test]
        public async Task Registers_consumer_type_supplied_to_consumerRegistrar()
        {
            await PublishMyEvent();
            WaitUntilBusHasProcessedMessageOrTimedOut(_manualResetEvent);

            Assert.That(_myEventConsumer.ReceivedMessage, Is.True);
            Assert.That(_myEventFaultConsumer.ReceivedMessage, Is.False);
        }

        private async Task PublishMyEvent()
        {
            await _busControl.Publish(new MyEvent());
        }

        private void WaitUntilBusHasProcessedMessageOrTimedOut(ManualResetEvent manualResetEvent)
        {
            manualResetEvent.WaitOne(TimeSpan.FromSeconds(5));
        }
    }
}