using System.Collections.Generic;
using MassTransit;

namespace MassTransitInMemoryTestingExample.Tests
{
    public static class State
    {
        static State()
        {
            CommandsReceived = new List<MyCommand>();
            CommandFaultsReceived = new List<Fault<MyCommand>>();
        }

        public static IList<MyCommand> CommandsReceived;
        public static IList<Fault<MyCommand>> CommandFaultsReceived;
    }
}