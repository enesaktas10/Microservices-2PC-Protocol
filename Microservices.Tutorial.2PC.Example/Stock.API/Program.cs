var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();

app.MapGet("/ready", () =>
{
    Console.WriteLine("Stock services is ready");
    return true;
});

app.MapGet("/commit", () =>
{
    Console.WriteLine("Stock services is commited");
    return true;
});

app.MapGet("/rollback", () =>
{
    Console.WriteLine("Stock services is rollbacked");
});

app.Run();