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
            EventsReceived = new List<MyEvent>();
            EventFaultsReceived = new List<Fault<MyEvent>>();
        }

        public static IList<MyCommand> CommandsReceived;
        public static IList<Fault<MyCommand>> CommandFaultsReceived;
        public static IList<MyEvent> EventsReceived;
        public static IList<Fault<MyEvent>> EventFaultsReceived;
    }
}