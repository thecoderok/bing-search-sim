
namespace BingSearchSim
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;

    class Program
    {
        static void Main(string[] args)
        {
            IWebDriver driver = new ChromeDriver();

            driver.Navigate().GoToUrl("https://www.bing.com/");
            IWebElement query = driver.FindElement(By.Name("q"));
            query.SendKeys("Cheese");
            System.Console.WriteLine("Page title is: " + driver.Title);
            driver.Quit();
        }
    }
}
