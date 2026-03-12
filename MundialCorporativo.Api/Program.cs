using Serilog;
using MundialCorporativo.Api.DependencyInjection;
using MundialCorporativo.Api.Middleware;
using MundialCorporativo.Api.Observability;
using MundialCorporativo.Application.Abstractions.Observability;
using MundialCorporativo.Infrastructure;
using MundialCorporativo.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext();
});

builder.Services.AddControllers()
    .AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationHandlers();
builder.Services.AddScoped<ITraceContext, HttpTraceContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationMiddleware>();
app.UseSerilogRequestLogging();

app.UseAuthorization();
app.MapControllers();

app.Run();
