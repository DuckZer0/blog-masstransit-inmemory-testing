using System;

namespace MassTransitInMemoryTestingExample
{
    public interface IConsumerFactory
    {
        object Create(Type typeToCreate);
    }
}