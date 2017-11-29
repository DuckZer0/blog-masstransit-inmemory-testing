using System;
using MassTransit;

namespace MassTransitInMemoryTestingExample
{
    public class ReceiveEndpoint
    {
        private readonly string _queueName;
        private readonly Action<IReceiveEndpointConfigurator> _configure;

        public ReceiveEndpoint(string queueName, Action<IReceiveEndpointConfigurator> configure)
        {
            _queueName = queueName;
            _configure = configure;
        }

        public void Register(IBusFactoryConfigurator busFactoryConfigurator)
        {
            busFactoryConfigurator.ReceiveEndpoint(_queueName, _configure);
        }
    }
}