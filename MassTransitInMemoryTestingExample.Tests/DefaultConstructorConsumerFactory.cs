using System;

namespace MassTransitInMemoryTestingExample.Tests
{
    public class DefaultConstructorConsumerFactory : IConsumerFactory
    {
        public object Create(Type typeToCreate)
        {
            return Activator.CreateInstance(typeToCreate);
        }
    }
}