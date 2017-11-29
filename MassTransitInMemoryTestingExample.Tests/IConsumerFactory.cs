using System;

namespace MassTransitInMemoryTestingExample.Tests
{
    public interface IConsumerFactory
    {
        object Create(Type typeToCreate);
    }
}