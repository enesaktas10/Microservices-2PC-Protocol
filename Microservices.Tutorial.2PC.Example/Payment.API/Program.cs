var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();

app.MapGet("/ready", () =>
{
    Console.WriteLine("Payment services is ready");
    return true;
});

app.MapGet("/commit", () =>
{
    Console.WriteLine("Payment services is commited");
    return true;
});

app.MapGet("/rollback", () =>
{
    Console.WriteLine("Payment services is rollbacked");
});

app.Run();