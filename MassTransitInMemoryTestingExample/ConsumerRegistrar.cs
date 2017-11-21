using System;
using MassTransit;

namespace MassTransitInMemoryTestingExample
{
    public class ConsumerRegistrar
    {
        private readonly Type[] _consumerTypes;
        private readonly Func<Type, IConsumer> _createConsumer;

        public ConsumerRegistrar(string queueName, Type[] consumerTypes, Func<Type, IConsumer> createConsumer)
        {
            QueueName = queueName;
            _consumerTypes = consumerTypes;
            _createConsumer = createConsumer;
        }

        public string QueueName { get; set; }

        public void ConfigureEndpoint(IReceiveEndpointConfigurator receiveEndpointConfigurator)
        {
            foreach (var consumerType in _consumerTypes)
            {
                receiveEndpointConfigurator.Consumer(consumerType, _createConsumer);
            }
        }
    }
}
