using System;
using MassTransit;

namespace MassTransitInMemoryTestingExample
{
    public class ConsumerRegistrar
    {
        private readonly Type[] _consumerTypes;
        private readonly Func<Type, IConsumer> _createConsumer;

        public ConsumerRegistrar(Type[] consumerTypes, Func<Type, IConsumer> createConsumer)
        {
            _consumerTypes = consumerTypes;
            _createConsumer = createConsumer;
        }

        public void ConfigureEndpoint(IReceiveEndpointConfigurator receiveEndpointConfigurator)
        {
            foreach (var consumerType in _consumerTypes)
            {
                receiveEndpointConfigurator.Consumer(consumerType, _createConsumer);
            }
        }
    }
}
