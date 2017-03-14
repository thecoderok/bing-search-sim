
namespace BingSearchSim
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using NLog;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;

    class Program
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private static readonly Random rnd = new Random();

        private const int NumOfDesktopSearches = 32;
        private const int NumOfMobileSearches = 22;

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

            ClickOnRewardedQueries(config);
        }

        private static void ClickOnRewardedQueries(ConfigReader config)
        {
            logger.Info("ClickOnRewardedQueries");
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("user-data-dir=" + config.Get("user-data-dir"));

            using (var driver = CreateWebDriver(options))
            {
                for (int i = 0; i < 5; i++)
                {
                    driver.Navigate().GoToUrl("https://www.bing.com/");
                    Thread.Sleep(300);
                    driver.FindElement(By.Id("id_rh")).Click();
                    Thread.Sleep(300);
                    driver.SwitchTo().Frame("bepfm");
                    var queriesProgress = driver.FindElements(By.XPath(".//*[.='0 of 10 points']/../*[@class='linkText']"));
                    logger.Info("Found {0} promotion queries", queriesProgress.Count);
                    if (queriesProgress.Count > 0)
                    {
                        logger.Info("Click on first reward query");
                        Thread.Sleep(300);
                        try
                        {
                            queriesProgress[0].Click();
                        }
                        catch (Exception e)
                        {
                            logger.Warn("Error when clicking on search query: {0}.", e.Message);
                        }
                    }
                    
                    Thread.Sleep(1000);
                }
                
                driver.Quit();
            }
            logger.Info("Done searching on Desktop");
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
            using (var driver = CreateWebDriver(options))
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

        private static IWebDriver CreateWebDriver(ChromeOptions options, bool retrying = false)
        {
            logger.Info("Attempting to create WebDriver, retrying: {0}.", retrying);
            if (retrying)
            {
                try
                {
                    KillTheProcesses();
                }
                catch (Exception e)
                {
                    logger.Error("Failed to kill the processes: {0}.", e.Message);
                }
            }
            
            try
            {
                IWebDriver result = new ChromeDriver(options);
                return result;
            }
            catch (Exception e)
            {
                logger.Error(e);
                if (!retrying)
                {
                    return CreateWebDriver(options, true);
                }
                else
                {
                    throw;
                }
            }
        }

        private static void KillTheProcesses()
        {
            var processesToKill = new List<string>()
            {
                "chromedriver",
                "chrome"
            };

            for (int i = 1; i <= 5; i++)
            {
                logger.Info("Killing the processes, iteration #{0}.", i);
                foreach (var procName in processesToKill)
                {
                    foreach (var process in Process.GetProcessesByName(procName))
                    {
                        logger.Warn("Killing the process {0} with PID {1}.", process.ProcessName, process.Id);
                        process.Kill();
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}
