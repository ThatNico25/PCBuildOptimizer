using ComputerBuilder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Microsoft.OpenApi.Models;
using PcBuilderModelAI_WebApi_ValuePrediction;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPredictionEnginePool<PcBuilderModelAI.ModelInput, PcBuilderModelAI.ModelOutput>()
    .FromFile("PcBuilderModelAI.mlnet");

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Description = "Docs for my API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API for value prediction!");
    });
}

app.MapPost("/bestBuild", (
    [FromBody] List<PCBuildData> builds,
    [FromQuery] string? category,
    [FromQuery] float? price,
    PredictionEnginePool <PcBuilderModelAI.ModelInput, PcBuilderModelAI.ModelOutput> pool) =>
{
    if (builds == null || builds.Count == 0)
        return Results.BadRequest("Build list is empty.");

    var results = builds.Where(b => b.title == category && b.total_price_usd <= price).Select(b =>
    {
        var input = new PcBuilderModelAI.ModelInput
        {
            Total_price_usd = b.total_price_usd,
            Cpu_compute_score = b.cpu_compute_score,
            Cpu_multicore_score = b.cpu_multicore_score,
            Cpu_efficiency_score = b.cpu_efficiency_score,
            Gpu_compute_score = b.gpu_compute_score,
            Gpu_power_efficiency = b.gpu_power_efficiency,
            Ram_capacity_score = b.ram_capacity_score,
            Ram_bandwidth_score = b.ram_bandwidth_score,
            Has_gpu = b.has_gpu
        };

        var prediction = pool.Predict(input);

        return new
        {
            Build = b,
            PerformancePerDollar = prediction.Score
        };
    });

    if (!results.Any())
    {
        return Results.BadRequest("No builds matched category/budget.");
    }

    var best = results.OrderByDescending(r => r.PerformancePerDollar).FirstOrDefault();
    return Results.Ok(best);
});

app.Run();
