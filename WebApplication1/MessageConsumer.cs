using MassTransit;

namespace WebApplication1;

public class Message
{
    public string Text { get; set; }
}

// This class and the MessageConsumer class below would usually obviously be in two
// different apps, but that's fine for demo purposes.
public class FaultMessageConsumer : IConsumer<Fault<Message>>
{
    private readonly ILogger<FaultMessageConsumer> logger;
    public FaultMessageConsumer(ILogger<FaultMessageConsumer> logger) => this.logger = logger;

    public Task Consume(ConsumeContext<Fault<Message>> context)
    {
        logger.LogWarning("FAULT HEADERS {Headers}", string.Join("; ", context.Headers.Select(h => $"{h.Key}, {h.Value}")));
        return Task.CompletedTask;
    }
}

public class MessageConsumer : IConsumer<Message>
{
    private readonly ILogger<MessageConsumer> logger;
    public MessageConsumer(ILogger<MessageConsumer> logger) => this.logger = logger;

    public Task Consume(ConsumeContext<Message> context)
    {
        logger.LogWarning("Consumer failing");
        throw new InvalidOperationException();
    }
}