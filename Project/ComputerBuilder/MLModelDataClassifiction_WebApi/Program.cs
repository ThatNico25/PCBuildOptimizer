using Microsoft.Extensions.ML;
using Microsoft.OpenApi.Models;
using MLModelDataClassifiction_WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

builder.Services.AddPredictionEnginePool<MLModelDataClassifiction.ModelInput, MLModelDataClassifiction.ModelOutput>()
    .FromFile("MLModelDataClassifiction.mlnet");

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API for data classification");
    });
}

app.MapPost("/classify", (
    [Microsoft.AspNetCore.Mvc.FromBody] FormFactorInput build) =>
    {
        if (build == null)
            return Results.BadRequest("Input is null.");

        var inputData = new MLModelDataClassifiction.ModelInput
        {
            Motherboard_form_factor = build.MotherboardFormFactor,
            Case_type = build.CaseType,
            Gpu_compute_score = build.GpuComputeScore,
            Cpu_form_factor = build.Cpu_form_factor
        };

        var predictions = MLModelDataClassifiction.PredictAllLabels(inputData);
        var totalScore = predictions.Sum(p => p.Value);

        var result = predictions.Select(p => new
        {
            Category = p.Key,
            Score = p.Value,
            ScorePercentage = totalScore > 0 ? Math.Round((p.Value / totalScore) * 100, 2) : 0
        }).ToList();

        return Results.Ok(result);
});

app.Run();
