using System;
using MassTransit;

namespace MassTransitInMemoryTestingExample
{
    public class ConsumerRegistrar
    {
        private readonly Func<Type, object> _create;
        private readonly Type[] _consumerTypes;

        public ConsumerRegistrar(Func<Type, object> create, params Type[] consumerTypes)
        {
            _create = create;
            _consumerTypes = consumerTypes;
        }

        public void Configure(IReceiveEndpointConfigurator receiveEndpointConfigurator)
        {
            _consumerTypes.ForEach(consumerType => receiveEndpointConfigurator.Consumer(consumerType, _create));
        }
    }
}