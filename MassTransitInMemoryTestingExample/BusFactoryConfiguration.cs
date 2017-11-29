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
            ConfigureConsumersListeningOnMainQueue(busFactoryConfigurator);
            ConfigureConsumersListeningOnErrorQueue(busFactoryConfigurator);
        }

        private void ConfigureConsumersListeningOnMainQueue(IBusFactoryConfigurator busFactoryConfigurator)
        {
            var receiveEndpoint = new ReceiveEndpoint(QueueName, typeof(MyCommandConsumer), typeof(MyEventConsumer));
            receiveEndpoint.Register(busFactoryConfigurator, _consumerFactory.Create);
        }

        private void ConfigureConsumersListeningOnErrorQueue(IBusFactoryConfigurator busFactoryConfigurator)
        {
            var receiveEndpoint = new ReceiveEndpoint(ErrorQueueName, typeof(MyCommandFaultConsumer), typeof(MyEventFaultConsumer));
            receiveEndpoint.Register(busFactoryConfigurator, _consumerFactory.Create);
        }
    }
}