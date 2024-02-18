using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TopWordsAnalyzer.Enums;
using TopWordsAnalyzer.Interfaces;
using TopWordsAnalyzer.Services;


var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddScoped<ITopWordsCounter, TopWordsCounter>()
    .AddScoped<IFileReaderFactory, FileReaderFactory>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://127.0.0.1:5500");
        });
});


var app = builder.Build();
app.UseCors();

app.MapPost("/file", (IFileReaderFactory fileReaderFactory, ITopWordsCounter topWordsCounter, [FromForm] Test test) =>
{
    //validator
    if (test.Tresholds is null)
    {
        return Results.BadRequest("No tresholds defined.");
    }

    if (test.file is null)
    {
        return Results.BadRequest("No file uploaded.");
    }

    int[] tresholds;
    try
    {
        tresholds = JsonConvert.DeserializeObject<int[]>(test.Tresholds);
    }
    catch (Exception e)
    {
        return Results.BadRequest("Tresholds param should be an array.");
    }

    var ext = Path.GetExtension(test.file.FileName).Replace(".", "");
    var isExtensionValid = Enum.TryParse<FileExtension>(ext, true, out var extension);

    if (!isExtensionValid)
    {
        return Results.BadRequest("Invalid file extension.");
    }

    string text;
    try
    {
        using var stream = test.file.OpenReadStream();
        var reader = fileReaderFactory.CreateReader(extension);
        text = reader.Read(stream);
    }
    catch (Exception ex)
    {
        return Results.BadRequest();
    }

    var topWordsResult = topWordsCounter.Count(text, tresholds);

    return Results.Ok(topWordsResult);
    
}).DisableAntiforgery();




app.Run();


public static class ExtensionMethods
{
    public static T DeepCopy<T>(this T self)
    {
        var serialized = JsonConvert.SerializeObject(self);
        return JsonConvert.DeserializeObject<T>(serialized);
    }
}

public class Test
{
    public IFormFile file { get; set; }
    public string pole { get; set; }
    public string Tresholds { get; set; }
}