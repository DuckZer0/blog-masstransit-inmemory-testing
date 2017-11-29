using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using NUnit.Framework;

namespace MassTransitInMemoryTestingExample.Tests
{
    [TestFixture]
    public class PublishTest
    {
        private IBusControl _busControl;
        private IConsumerFactory _consumerFactory;
        private BusFactoryConfiguration _busFactoryConfiguration;

        [SetUp]
        public void SetUp()
        {
            _consumerFactory = new DefaultConstructorConsumerFactory();
            _busFactoryConfiguration = new BusFactoryConfiguration(_consumerFactory);
            CreateBus();
            _busControl.Start();
        }

        private void CreateBus()
        {
            _busControl = Bus.Factory.CreateUsingInMemory(_busFactoryConfiguration.Configure);
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