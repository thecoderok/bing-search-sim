namespace BingSearchSim
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class ConfigReader
    {
        private readonly Dictionary<string, string> config = new Dictionary<string, string>();

        public ConfigReader()
        {
            var lines = File.ReadAllLines("config.ini");
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var split = line.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                config.Add(split[0], split[1]);
            }
        }

        public string Get(string key)
        {
            return config[key];
        }
    }
}
