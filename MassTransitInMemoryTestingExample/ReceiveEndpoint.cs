using System;
using MassTransit;

namespace MassTransitInMemoryTestingExample
{
    public class ReceiveEndpoint
    {
        private readonly string _queueName;
        private readonly Type[] _consumerTypes;

        public ReceiveEndpoint(string queueName, params Type[] consumerTypes)
        {
            _queueName = queueName;
            _consumerTypes = consumerTypes;
        }

        public void Register(IBusFactoryConfigurator busFactoryConfigurator, Func<Type, object> create)
        {
            var consumerRegistrar = new ConsumerRegistrar(create, _consumerTypes);
            busFactoryConfigurator.ReceiveEndpoint(
                _queueName,
                consumerRegistrar.Configure);
        }
    }
}