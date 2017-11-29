using System;
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
            var consumerTypes = new[] { typeof(MyCommandConsumer), typeof(MyEventConsumer) };
            RegisterConsumers(busFactoryConfigurator, QueueName, consumerTypes);
        }

        private void ConfigureConsumersListeningOnErrorQueue(IBusFactoryConfigurator busFactoryConfigurator)
        {
            var consumerTypes = new[] { typeof(MyCommandFaultConsumer), typeof(MyEventFaultConsumer) };
            RegisterConsumers(busFactoryConfigurator, ErrorQueueName, consumerTypes);
        }

        private void RegisterConsumers(IBusFactoryConfigurator busFactoryConfigurator, string queueName, Type[] consumerTypes)
        {
            busFactoryConfigurator.ReceiveEndpoint(queueName,
                receiveEndpointConfigurator =>
                {
                    foreach (var consumerType in consumerTypes)
                    {
                        receiveEndpointConfigurator.Consumer(consumerType, _consumerFactory.Create);
                    }
                });
        }
    }
}