using MassTransit;
using MassTransit.Log4NetIntegration;

namespace MassTransitInMemoryTestingExample.Tests
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
            ConfigureConsumersListeningOnMainQueue(busFactoryConfigurator);
            ConfigureConsumersListeningOnErrorQueue(busFactoryConfigurator);
        }

        private void ConfigureConsumersListeningOnMainQueue(IBusFactoryConfigurator busFactoryConfigurator)
        {
            busFactoryConfigurator.ReceiveEndpoint(QueueName,
                receiveEndpointConfigurator =>
                {
                    receiveEndpointConfigurator.Consumer(typeof(MyCommandConsumer), _consumerFactory.Create);
                    receiveEndpointConfigurator.Consumer(typeof(MyEventConsumer), _consumerFactory.Create);
                });
        }

        private void ConfigureConsumersListeningOnErrorQueue(IBusFactoryConfigurator busFactoryConfigurator)
        {
            busFactoryConfigurator.ReceiveEndpoint(ErrorQueueName,
                receiveEndpointConfigurator =>
                {
                    receiveEndpointConfigurator.Consumer(typeof(MyCommandFaultConsumer), _consumerFactory.Create);
                    receiveEndpointConfigurator.Consumer(typeof(MyEventFaultConsumer), _consumerFactory.Create);
                });
        }
    }
}