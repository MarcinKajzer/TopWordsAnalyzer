using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using TopWordsAnalyzer.Enums;
using TopWordsAnalyzer.Interfaces;
using TopWordsAnalyzer.Readers;

namespace TopWordsAnalyzer.Services;

public class TopWordsService : ITopWordsService
{
    private readonly IFileReaderFactory _fileReaderFactory;
    private readonly ITopWordsCounter _topWordsCounter;
    private readonly IMemoryCache _memoryCache;

    public TopWordsService(IFileReaderFactory fileReaderFactory, ITopWordsCounter topWordsCounter, IMemoryCache memoryCache)
    {
        _fileReaderFactory = fileReaderFactory;
        _topWordsCounter = topWordsCounter;
        _memoryCache = memoryCache;
    }

    public IResult AnalyzeFile(ReportFormData formData)
    {
        //validator
        if (formData.Tresholds is null)
        {
            return Results.BadRequest("No tresholds defined.");
        }

        if (formData.File is null)
        {
            return Results.BadRequest("No file uploaded.");
        }

        int[] tresholds;
        try
        {
            tresholds = JsonConvert.DeserializeObject<int[]>(formData.Tresholds);
        }
        catch (Exception e)
        {
            return Results.BadRequest("Tresholds param should be an array.");
        }

        var ext = Path.GetExtension(formData.File.FileName).Replace(".", "");
        var isExtensionValid = Enum.TryParse<FileExtension>(ext, true, out var extension);

        if (!isExtensionValid)
        {
            return Results.BadRequest("Invalid file extension.");
        }

        string text;
        try
        {
            using var stream = formData.File.OpenReadStream();
            var reader = _fileReaderFactory.CreateReader(extension);
            text = reader.Read(stream);
        }
        catch (Exception ex)
        {
            return Results.BadRequest();
        }

        var topWordsResult = _topWordsCounter.Count(text, tresholds);

        //var cacheKey = Guid.NewGuid();
        var cacheKey = "dd5de719-f38a-4703-b5da-87a895e40798";
        _memoryCache.Set(cacheKey, topWordsResult, TimeSpan.FromMinutes(30));

        //topWordsResult.CacheKey = cacheKey;
        return Results.Ok(topWordsResult);
    }
}
