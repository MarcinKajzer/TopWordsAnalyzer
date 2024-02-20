namespace TopWordsAnalyzer.Model;

public class Report
{
    public int AllWordsCount { get; set; }
    public int AllUniqueWordsCount { get; set; }
    public Dictionary<string, int> WordsOccurriances { get; set; }
    public List<Treshold> Tresholds { get; set; } = new();
    public Guid ReportId { get; set; }
}