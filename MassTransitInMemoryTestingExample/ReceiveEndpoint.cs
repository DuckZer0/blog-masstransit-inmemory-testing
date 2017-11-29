using System;
using MassTransit;

namespace MassTransitInMemoryTestingExample
{
    public class ReceiveEndpoint
    {
        private readonly Func<Type, object> _create;
        private readonly string _queueName;
        private readonly Type[] _consumerTypes;

        public ReceiveEndpoint(Func<Type, object> create, string queueName, params Type[] consumerTypes)
        {
            _create = create;
            _queueName = queueName;
            _consumerTypes = consumerTypes;
        }

        public void Register(IBusFactoryConfigurator busFactoryConfigurator)
        {
            busFactoryConfigurator.ReceiveEndpoint(_queueName,
                receiveEndpointConfigurator =>
                {
                    foreach (var consumerType in _consumerTypes)
                    {
                        receiveEndpointConfigurator.Consumer(consumerType, _create);
                    }
                });
        }
    }
}