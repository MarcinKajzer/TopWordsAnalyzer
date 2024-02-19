using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TopWordsAnalyzer.Helpers;
using TopWordsAnalyzer.Interfaces;
using TopWordsAnalyzer.Model;
using TopWordsAnalyzer.Readers;
using TopWordsAnalyzer.Services;


var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddScoped<ITopWordsCounter, TopWordsCounter>()
    .AddScoped<IFileReaderFactory, FileReaderFactory>()
    .AddScoped<IXlsxReportGenerator, XlsxReportGenerator>()
    .AddScoped<ITopWordsService, TopWordsService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://127.0.0.1:5500");
        });
});

builder.Services.AddMemoryCache();

var app = builder.Build();
app.UseCors();



app.MapGet("/download", (IMemoryCache memoryCache, IXlsxReportGenerator xlsxGenerator, Guid? cacheKey) =>
{
    if (!memoryCache.TryGetValue<Report>("dd5de719-f38a-4703-b5da-87a895e40798", out var data))
    {
        return Results.BadRequest("Raport wygas³.");
    }

    var excel = xlsxGenerator.Generate(data);
    return Results.File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "output.xlsx");    
});

app.MapPost("/file", (ITopWordsService reportService, [FromForm] ReportFormData formData) =>
{
    return reportService.AnalyzeFile(formData);

}).DisableAntiforgery();



app.Run();
