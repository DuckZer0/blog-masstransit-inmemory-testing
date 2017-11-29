using System.Threading.Tasks;
using MassTransit;

namespace MassTransitInMemoryTestingExample.Tests
{
    public class MyEventConsumer
        : IConsumer<MyEvent>
    {
        public async Task Consume(ConsumeContext<MyEvent> context)
        {
            State.EventsReceived.Add(context.Message);
        }
    }
}