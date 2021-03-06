﻿using System.Threading.Tasks;
using MassTransit;

namespace MassTransitInMemoryTestingExample
{
    public class MyCommandFaultConsumer
        : IConsumer<Fault<MyCommand>>
    {
        public async Task Consume(ConsumeContext<Fault<MyCommand>> context)
        {
            State.CommandFaultsReceived.Add(context.Message);
        }
    }
}