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
                new ReceiveEndpoint(QueueName, typeof(MyCommandConsumer), typeof(MyEventConsumer)),
                new ReceiveEndpoint(ErrorQueueName, typeof(MyCommandFaultConsumer), typeof(MyEventFaultConsumer))
            };

            receiveEndpoints.ForEach(receiveEndpoint => receiveEndpoint.Register(busFactoryConfigurator, _consumerFactory.Create));
        }
    }
}