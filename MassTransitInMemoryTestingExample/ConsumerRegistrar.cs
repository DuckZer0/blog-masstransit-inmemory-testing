using System;
using MassTransit;

namespace MassTransitInMemoryTestingExample
{
    public class ConsumerRegistrar
    {
        private readonly string _queueName;
        private readonly IConsumerFactory _consumerFactory;
        private readonly Type[] _consumerTypes;

        public ConsumerRegistrar(IConsumerFactory consumerFactory, string queueName, params Type[] consumerTypes)
        {
            _queueName = queueName;
            _consumerFactory = consumerFactory;
            _consumerTypes = consumerTypes;
        }

        public void Register(IBusFactoryConfigurator busFactoryConfigurator)
        {
            busFactoryConfigurator.ReceiveEndpoint(_queueName,
                receiveEndpointConfigurator =>
                {
                    foreach (var consumerType in _consumerTypes)
                    {
                        receiveEndpointConfigurator.Consumer(consumerType, _consumerFactory.Create);
                    }
                });
        }
    }
}