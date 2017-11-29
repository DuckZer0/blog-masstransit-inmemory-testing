using System.Threading.Tasks;
using MassTransit;

namespace MassTransitInMemoryTestingExample.Tests
{
    public class MyEventFaultConsumer
        : IConsumer<Fault<MyEvent>>
    {
        public async Task Consume(ConsumeContext<Fault<MyEvent>> context)
        {
            State.EventFaultsReceived.Add(context.Message);
        }
    }
}