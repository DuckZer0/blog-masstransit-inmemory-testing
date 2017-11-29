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
    public class SendTest
    {
        private const string QueueName = "myQueue";
        private const string ErrorQueueName = "myQueue_error";
        private const string LoopbackAddress = "loopback://localhost/";
        private IBusControl _busControl;
        private MyCommandConsumer _myCommandConsumer;
        private MyCommandFaultConsumer _myCommandFaultConsumer;

        [SetUp]
        public void SetUp()
        {
            _myCommandConsumer = new MyCommandConsumer();
            _myCommandFaultConsumer = new MyCommandFaultConsumer();
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
                receiveEndpointConfigurator.Consumer(typeof(MyCommandConsumer), consumerType => _myCommandConsumer);
            });
        }

        private void ConfigureReceiveEndpointToListenForFaults(IBusFactoryConfigurator busFactoryConfigurator)
        {
            busFactoryConfigurator.ReceiveEndpoint(ErrorQueueName, receiveEndpointConfigurator =>
            {
                receiveEndpointConfigurator.Consumer(typeof(MyCommandFaultConsumer), type => _myCommandFaultConsumer);
            });
        }

        [Test]
        public async Task Consumer_has_been_registered_to_receive_message()
        {
            await SendMyCommand();
            WaitUntilConditionMetOrTimedOut(() => State.CommandsReceived.Any());

            Assert.That(State.CommandsReceived.Count, Is.EqualTo(1));
            Assert.That(State.CommandFaultsReceived.Count, Is.EqualTo(0));
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
