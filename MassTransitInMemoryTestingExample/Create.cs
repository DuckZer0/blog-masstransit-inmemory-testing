using System;

namespace MassTransitInMemoryTestingExample
{
    public static class Create
    {
        public static ReceiveEndpointFactory ReceiveEndpoint => new ReceiveEndpointFactory();

        public class ReceiveEndpointFactory
        {
            private Func<Type, object> _create;
            public ReceiveEndpointFactory WithConsumerFactory(Func<Type, object> create)
            {
                _create = create;
                return this;
            }

            public ReceiveEndpointDescriptor WithConsumers(params Type[] types)
            {
                return new ReceiveEndpointDescriptor(_create, types);
            }
        }

        public class ReceiveEndpointDescriptor
        {
            private readonly Func<Type, object> _create;
            private readonly Type[] _consumerTypes;

            public ReceiveEndpointDescriptor(Func<Type, object> create, Type[] consumerTypes)
            {
                _create = create;
                _consumerTypes = consumerTypes;
            }

            public ReceiveEndpoint ListeningOn(string queueName)
            {
                var consumerRegistrar = new ConsumerRegistrar(_create, _consumerTypes);
                return new ReceiveEndpoint(queueName, consumerRegistrar.Configure);
            }
        }
    }
}