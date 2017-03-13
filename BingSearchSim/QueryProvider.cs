namespace BingSearchSim
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class QueryProvider
    {
        private readonly List<string> queries;
        private readonly Random random = new Random();
        private const int MaxQueryLen = 30;

        public QueryProvider()
        {
            string[] lines = File.ReadAllLines("Queries.txt");
            queries = new List<string>(lines);
        }

        public string GetNextRandomQuery()
        {
            var index1 = random.Next(0, queries.Count-1);
            var index2 = random.Next(0, queries.Count-1);
            string fullQuery = queries[index1] + " " + queries[index2];
            if (fullQuery.Length > MaxQueryLen)
            {
                fullQuery = fullQuery.Substring(0, MaxQueryLen);
            }

            return fullQuery;
        }
    }
}
