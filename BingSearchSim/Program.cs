
namespace BingSearchSim
{
    using System;
    using System.Threading;
    using NLog;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;

    class Program
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private static readonly Random rnd = new Random();

        private const int NumOfDesktopSearches = 30;
        private const int NumOfMobileSearches = 20;

        static void Main(string[] args)
        {
            logger.Info("Application is running");
            try
            {
                Run();
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
            logger.Info("Completed");
        }

        private static void Run()
        {
            var queryProvider = new QueryProvider();
            var config = new ConfigReader();

           DoDesktopSearches(config, queryProvider);

            DoMobileSearches(config, queryProvider);
        }

        private static void DoMobileSearches(ConfigReader config, QueryProvider queryProvider)
        {
            logger.Info("Searching on mobile");
            const string url = "https://m.bing.com/";
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("user-data-dir=" + config.Get("user-data-dir"));
            options.AddArgument("user-agent=" + config.Get("MobileUserAgent"));
            SearchGeneric(queryProvider, options, url, NumOfMobileSearches);
            logger.Info("Done searching on mobile");
        }

        private static void SearchGeneric(QueryProvider queryProvider, ChromeOptions options, string url, int numOfSearches)
        {
            using (var driver = new ChromeDriver(options))
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                for (int i = 0; i < NumOfMobileSearches; i++)
                {
                    MakeSearch(queryProvider.GetNextRandomQuery(), driver, url);
                    int sleepSeconds = rnd.Next(2, 10);
                    logger.Info("Sleeping for {0} seconds.", sleepSeconds);
                    Thread.Sleep(sleepSeconds * 1000);
                }

                driver.Quit();
            }
        }

        private static void DoDesktopSearches(ConfigReader config, QueryProvider queryProvider)
        {
            logger.Info("Searching on Desktop");
            const string url = "https://www.bing.com/";
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("user-data-dir=" + config.Get("user-data-dir"));
            SearchGeneric(queryProvider, options, url, NumOfDesktopSearches);
            logger.Info("Done searching on Desktop");
        }

        private static void MakeSearch(string query, IWebDriver driver, string url)
        {
            logger.Info("Performing search: q='{0}'", query);
            driver.Navigate().GoToUrl(url);
            IWebElement queryBox = driver.FindElement(By.Name("q"));
            queryBox.SendKeys(query);
            queryBox.SendKeys(Keys.Enter);
            logger.Info("Search complete");
        }
    }
}
