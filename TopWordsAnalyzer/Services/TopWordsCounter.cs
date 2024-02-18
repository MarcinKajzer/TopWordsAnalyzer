﻿using System.Text.RegularExpressions;
using TopWordsAnalyzer.Interfaces;
using TopWordsAnalyzer.Model;

namespace TopWordsAnalyzer.Services
{
    public class TopWordsCounter : ITopWordsCounter
    {
        public Result Count(string text, int[] percentageTresholds)
        {
            text = RemovePunctuation(text).ToLower();
            var words = ConvertToArr(text);
            var wordOccurrences = CountWordOccurrences(words);

            return CountOccurencesForPercentageTresholds(wordOccurrences, words.Length, percentageTresholds);
        }


        private static string RemovePunctuation(string text)
        {
            return Regex.Replace(text, @"[^\w\s\r]", " ").Replace("\n", " ");
        }

        private static string[] ConvertToArr(string text)
        {
            return text.Split(" ").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        }

        private static Dictionary<string, int> CountWordOccurrences(string[] array)
        {
            Dictionary<string, int> occurrences = new Dictionary<string, int>();

            foreach (string str in array)
            {
                if (occurrences.ContainsKey(str))
                {
                    occurrences[str]++;
                }
                else
                {
                    occurrences[str] = 1;
                }
            }

            return occurrences.OrderByDescending(o => o.Value).ToDictionary();
        }

        private static Result CountOccurencesForPercentageTresholds(Dictionary<string, int> occurrences, int totalCount, int[] percentageTresholds)
        {
            var sortedKeys = occurrences.Keys.ToArray();

            int totalOcurrences = 0;
            int tresholdIndex = 0;

            var currentTreshold = new Treshold(percentageTresholds[tresholdIndex]);
            int tresholdElementsCount = (percentageTresholds[tresholdIndex] * totalCount) / 100;

            List<Treshold> tresholds = new List<Treshold>();

            foreach (string key in sortedKeys)
            {
                totalOcurrences += occurrences[key];

                if (totalOcurrences <= tresholdElementsCount)
                {
                    currentTreshold.Words.Add(key);
                }
                else
                {
                    currentTreshold.OccurrencesCount = totalOcurrences;
                    currentTreshold.PercentOfUniqueWords = 100 * currentTreshold.Words.Count() / sortedKeys.Count();
                    currentTreshold.Words.Sort();
                    tresholds.Add(currentTreshold);

                    if (tresholdIndex == percentageTresholds.Count() - 1)
                    {
                        break;
                    }

                    tresholdIndex++;
                    tresholdElementsCount = (percentageTresholds[tresholdIndex] * totalCount) / 100;
                    currentTreshold = currentTreshold.DeepCopy();
                    currentTreshold.PercentOfAllWords = percentageTresholds[tresholdIndex];
                }
            }

            var result = new Result
            {
                AllWordsCount = totalCount,
                AllUniqueWordsCount = sortedKeys.Count(),
                Tresholds = tresholds,
                WordsOccurriances = occurrences
            };

            return result;
        }
    }
}
