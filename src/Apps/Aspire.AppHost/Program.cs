var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("rabbitmq-username", "guest");
var password = builder.AddParameter("rabbitmq-password", "guest");

var rabbitmq = builder
    .AddRabbitMQ("rabbitmq", username, password)
    .WithManagementPlugin()
    .WithEndpoint("tcp", endpoint =>
    {
        endpoint.Port = 5672;
        endpoint.TargetPort = 5672;
    })
    .WithEndpoint("management", endpoint =>
    {
        endpoint.Port = 15672;
        endpoint.TargetPort = 15672;
    });

//builder.AddProject<Projects.RabbitMQ_Console_Tests>("rabbitmq-console-tests")
//    .WithReference(rabbitmq)
//    .WaitFor(rabbitmq);

builder.AddProject<Projects.RabbitMQ_ConsumerDataflowService>("consumer-dataflow-service")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

//builder.AddProject<Projects.OpenTelemetry_Console_Tests>("otel-console-tests")
//    .WithReference(rabbitmq)
//    .WaitFor(rabbitmq);

builder.Build().Run();
