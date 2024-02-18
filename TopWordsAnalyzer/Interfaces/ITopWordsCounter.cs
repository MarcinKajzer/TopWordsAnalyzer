using TopWordsAnalyzer.Model;

namespace TopWordsAnalyzer.Interfaces;

public interface ITopWordsCounter
{
    Result Count(string text, int[] percentageTresholds);
}

