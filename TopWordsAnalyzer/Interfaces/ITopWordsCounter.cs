using TopWordsAnalyzer.Model;

namespace TopWordsAnalyzer.Interfaces;

public interface ITopWordsCounter
{
    Report Count(string text, int[] percentageTresholds);
}

