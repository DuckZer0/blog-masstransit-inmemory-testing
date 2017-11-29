using System;

namespace MassTransitInMemoryTestingExample
{
    public static class Create
    {
        public static ReceiveEndpointFactory ReceiveEndpoint => new ReceiveEndpointFactory();

        public class ReceiveEndpointFactory
        {
            public ReceiveEndpointDescriptor WithConsumers(params Type[] types)
            {
                return new ReceiveEndpointDescriptor(types);
            }
        }

        public class ReceiveEndpointDescriptor
        {
            private readonly Type[] _consumerTypes;

            public ReceiveEndpointDescriptor(Type[] consumerTypes)
            {
                _consumerTypes = consumerTypes;
            }

            public ReceiveEndpoint ListeningOn(string queueName)
            {
                return new ReceiveEndpoint(queueName, _consumerTypes);
            }
        }
    }
}