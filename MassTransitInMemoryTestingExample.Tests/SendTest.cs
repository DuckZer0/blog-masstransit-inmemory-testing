using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing.Observers;
using NUnit.Framework;

namespace MassTransitInMemoryTestingExample.Tests
{
    [TestFixture]
    public class SendTest
    {
        private const string QueueName = "myQueue";
        private const string LoopbackAddress = "loopback://localhost/";
        private IBusControl _busControl;
        private IConsumerFactory _consumerFactory;
        private BusFactoryConfiguration _busFactoryConfiguration;
        private BusTestConsumeObserver _busTestConsumeObserver;

        [SetUp]
        public void SetUp()
        {
            _consumerFactory = new DefaultConstructorConsumerFactory();
            _busFactoryConfiguration = new BusFactoryConfiguration(_consumerFactory);
            CreateBus();
            _busControl.Start();
            _busTestConsumeObserver = new BusTestConsumeObserver(TimeSpan.FromSeconds(10));
            _busControl.ConnectConsumeObserver(_busTestConsumeObserver);
        }

        private void CreateBus()
        {
            _busControl = Bus.Factory.CreateUsingInMemory(_busFactoryConfiguration.Configure);
        }

        [Test]
        public async Task Consumer_has_been_registered_to_receive_command()
        {
            await SendMyCommand();
            WaitUntilConditionMetOrTimedOut(() => State.CommandsReceived.Any());

            Assert.That(State.CommandsReceived.Count, Is.EqualTo(1));
            Assert.That(State.CommandFaultsReceived.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task Consumer_has_been_registered_to_receive_command_alternative_approach()
        {
            await SendMyCommand();
            WaitUntilConditionMetOrTimedOut(() => State.CommandsReceived.Any()); // test fails if we don't give consuming thread a chance to execute.

            Assert.That(_busTestConsumeObserver.Messages.Count, Is.EqualTo(1));
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
