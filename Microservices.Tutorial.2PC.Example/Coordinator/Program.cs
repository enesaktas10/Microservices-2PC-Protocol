using Coordinator.Modes.Contexts;
using Coordinator.Services;
using Coordinator.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TwoPhaseCommitContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("connectionString")));

builder.Services.AddHttpClient("OrderAPI", client => client.BaseAddress = new Uri("https://localhost:7085/"));
builder.Services.AddHttpClient("PaymentAPI", client => client.BaseAddress = new Uri("https://localhost:7146/"));
builder.Services.AddHttpClient("StockAPI", client => client.BaseAddress = new Uri("https://localhost:7004/"));

builder.Services.AddSingleton<ITransactionService, TransactionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
