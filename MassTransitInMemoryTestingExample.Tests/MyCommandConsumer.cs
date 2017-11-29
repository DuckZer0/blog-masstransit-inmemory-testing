using System.Threading.Tasks;
using MassTransit;

namespace MassTransitInMemoryTestingExample.Tests
{
    public class MyCommandConsumer
        : IConsumer<MyCommand>
    {
        public async Task Consume(ConsumeContext<MyCommand> context)
        {
            State.CommandsReceived.Add(context.Message);
        }
    }
}