using System;
using Castle.Windsor;

namespace MassTransitInMemoryTestingExample.Tests
{
    public class WindsorConsumerFactory : IConsumerFactory
    {
        private readonly IWindsorContainer _windsorContainer;

        public WindsorConsumerFactory(IWindsorContainer windsorContainer)
        {
            _windsorContainer = windsorContainer;
        }

        public object Create(Type typeToCreate)
        {
            return _windsorContainer.Resolve(typeToCreate);
        }
    }
}