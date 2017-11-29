using MassTransit;
using MassTransit.Log4NetIntegration;

namespace MassTransitInMemoryTestingExample
{
    public class BusFactoryConfiguration
    {
        private readonly IConsumerFactory _consumerFactory;
        private const string QueueName = "myQueue";
        private const string ErrorQueueName = "myQueue_error";

        public BusFactoryConfiguration(IConsumerFactory consumerFactory)
        {
            _consumerFactory = consumerFactory;
        }

        public void Configure(IBusFactoryConfigurator busFactoryConfigurator)
        {
            busFactoryConfigurator.UseLog4Net();
            ConfigureReceiveEndpoints(busFactoryConfigurator);
        }

        private void ConfigureReceiveEndpoints(IBusFactoryConfigurator busFactoryConfigurator)
        {
            var receiveEndpoints = new[]
            {
                Create.ReceiveEndpoint.WithConsumers(typeof(MyCommandConsumer), typeof(MyEventConsumer)).ListeningOn(QueueName),
                Create.ReceiveEndpoint.WithConsumers(typeof(MyCommandFaultConsumer), typeof(MyEventFaultConsumer)).ListeningOn(ErrorQueueName)
            };

            receiveEndpoints.ForEach(receiveEndpoint => receiveEndpoint.Register(busFactoryConfigurator, _consumerFactory.Create));
        }
    }
}