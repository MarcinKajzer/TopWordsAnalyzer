using Microsoft.AspNetCore.Mvc;
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
            policy.AllowAnyHeader();
        });
});

builder.Services.AddMemoryCache();

var app = builder.Build();
app.UseCors();



app.MapGet("/report", (ITopWordsService reportService, string reportId) =>
{
    return reportService.GetXlsxReport(reportId);
});

app.MapPost("/file", (ITopWordsService reportService, [FromForm] ReportFileData fileData) =>
{
    return reportService.AnalyzeFile(fileData);

}).DisableAntiforgery();

app.MapPost("/text", (ITopWordsService reportService, [FromBody] ReportTextData textData) =>
{
    return reportService.AnalyzeText(textData);

}).DisableAntiforgery();

app.Run();
