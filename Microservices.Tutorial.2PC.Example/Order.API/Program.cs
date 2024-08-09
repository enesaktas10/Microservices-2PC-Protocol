var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();

app.MapGet("/ready", () =>
{
    Console.WriteLine("Order services is ready");
    return true;
    //return false; ready durumunu basarisiza cekrek test yapabilirsin
});

app.MapGet("/commit", () =>
{
    Console.WriteLine("Order services is commited");
    return true;
});

app.MapGet("/rollback", () =>
{
    Console.WriteLine("Order services is rollbacked");
});

app.Run();
