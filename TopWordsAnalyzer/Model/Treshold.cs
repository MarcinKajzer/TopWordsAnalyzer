namespace TopWordsAnalyzer.Model;

public class Treshold
{
    public int PercentOfAllWords { get; set; }
    public int PercentOfUniqueWords { get; set; }
    public int WordsCount => Words.Count();
    public int OccurrencesCount { get; set; }
    public List<string> Words { get; set; } = new();

    public Treshold(int percentOfAllWords) =>  PercentOfAllWords = percentOfAllWords;
}
