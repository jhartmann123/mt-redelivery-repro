using System.Reflection;
using Amazon;
using Amazon.Runtime;
using MassTransit;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(o =>
{
    o.AddConsumers(Assembly.GetExecutingAssembly());

    // Amazon SQS
    //o.UsingAmazonSqs((context, cfg) => 
    //{
    //    cfg.Host(RegionEndpoint.EUCentral1.SystemName, h =>
    //    {
    //        h.Credentials(new SessionAWSCredentials(
    //            ));
    //
    //        h.Scope("dev-masstransit-repro");
    //        h.EnableScopedTopics();
    //    });

    // RabbitMQ
    o.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri("rabbitmq://localhost/"));

        cfg.ReceiveEndpoint("testqueue", e =>
        {
            // Configuring the MessageRetry here causes additional fault messages to be sent
            //e.UseMessageRetry(retry =>
            //{
            //    retry.Handle<ArgumentException>();
            //    retry.Interval(3, TimeSpan.FromSeconds(3));
            //});

            e.ConfigureConsumer<MessageConsumer>(context, consumer =>
            {
                // Configuring the MessageRetry here causes additional fault messages to be sent

                consumer.UseDelayedRedelivery(redelivery =>
                {
                    redelivery.Handle<InvalidOperationException>();
                    redelivery.Interval(3, TimeSpan.FromSeconds(3));
                });

                // This is the only valid place where message retries can be configured
                // without generating additional Fault<T> messages. But in our case
                // I want a "global" retry for transient DbExceptions
                consumer.UseMessageRetry(retry =>
                {
                    retry.Handle<ArgumentException>();
                    retry.Interval(3, TimeSpan.FromSeconds(3));
                });
            });

            // Configuring the MessageRetryMessageRetry here causes additional fault messages to be sent

            e.ConfigureConsumers(context);
        });

    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/publish", (IPublishEndpoint ep) =>
{
    ep.Publish(new Message { Text = "Hi" });
}).WithOpenApi();

app.Run();
