using Rpa.Curso.CrossCutting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace Rpa.Curso.Repository
{
    public class WebDriverRepository : LoggingBase, IDisposable
    {
        private bool disposedValue;
        protected readonly IWebDriver _driver;
        protected WebDriverRepository(bool installChromeDriver)
        {
            if (installChromeDriver)
            {
                var chromeDriverInstaller = new ChromeDriverInstaller();
                var chromeVersion = chromeDriverInstaller.GetChromeVersion().GetAwaiter().GetResult();
                chromeDriverInstaller.Install(chromeVersion).GetAwaiter().GetResult();
            }
            this._driver = new ChromeDriver(this.GetChromeOptions());
        }

        protected void NavigateToUrl(string url)
        {
            this._driver.Navigate().GoToUrl(url);
        }

        protected void ExecuteJavaScript(string script)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)this._driver;
            js.ExecuteScript(script);
        }

        protected string ExecuteJavaScriptWithResult(string script)
        {
            if (!script.ToLower().Contains("return"))
            {
                throw new InvalidOperationException("O parâmetro 'script' precisa conter a expressão 'return'.");
            }
            IJavaScriptExecutor js = (IJavaScriptExecutor)this._driver;
            return (js.ExecuteScript(script) as string);
        }

        protected void MoveToElement(string xPath)
        {
            Actions action = new Actions(this._driver);
            action.MoveToElement(this.GetElementByXPath(xPath)).Perform();
        }

        protected void PerformOneClickOnElement(string xPath)
        {
            this.GetElementByXPath(xPath).Click();
        }

        protected void PerformDoubleClickOnElement(string xPath)
        {
            Actions act = new Actions(this._driver);
            act.DoubleClick(this.GetElementByXPath(xPath)).Perform();
        }

        protected void PerformDoubleClickOnElement(IWebElement webElement)
        {
            Actions act = new Actions(this._driver);
            act.DoubleClick(webElement).Perform();
        }

        protected void AddTextIntoInput(string xPath, string text)
        {
            this.GetElementByXPath(xPath).Clear();
            this.GetElementByXPath(xPath).SendKeys(text);
        }

        protected string GetElementAttributeValue(string xPath, string attributeName)
        {
            return this.GetElementByXPath(xPath).GetAttribute(attributeName);
        }

        protected string GetElementText(string xPath)
        {
            return this.GetElementByXPath(xPath).Text;
        }

        protected bool CheckIfElementIsEnable(string xPath)
        {
            return this.GetElementByXPath(xPath).Enabled;
        }

        protected IWebElement GetElementByXPath(string xPath)
        {
            return this._driver.FindElement(By.XPath(xPath));
        }

        protected IEnumerable<IWebElement> GetElementsByXPath(string xPath)
        {
            return this._driver.FindElements(By.XPath(xPath));
        }

        protected void CheckIfElementIsLoaded(string xpath, int? sleep = null)
        {
            if (null == sleep)
            {
                sleep = 10000;
            }
            int counter = 0;
            IWebElement element = null;
            while (element == null)
            {
                try
                {
                    element = this._driver.FindElement(By.XPath(xpath));
                }
                catch
                {
                    counter++;
                    if (counter < sleep)
                    {
                        continue;
                    }
                    else
                    {
                        throw new InvalidOperationException("Timeout, Surgiu um erro ao tentar navegar nos elementos do sistema, vide XPATH: " + xpath);
                    }
                }
            }
        }

        protected bool CheckIfElementIsLoadedWithResult(string xpath, int? sleep = null)
        {
            bool result = false;
            if (null == sleep)
            {
                sleep = 10000;
            }
            int counter = 0;
            IWebElement element = null;
            while (element == null)
            {
                try
                {
                    element = this._driver.FindElement(By.XPath(xpath));
                    result = true;
                }
                catch
                {
                    counter++;
                    if (counter < sleep)
                    {
                        continue;
                    }
                    else
                    {
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && null != this._driver)
                {
                    this._driver.Dispose();
                }
                disposedValue = true;
            }
        }

        ~WebDriverRepository()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private ChromeOptions GetChromeOptions()
        {
            ChromeOptions options = new();

            //options.AddArgument("--headless");
            options.AddArgument("--disable-webgl");
            options.AddArgument("--disable-software-rasterizer");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--enable-automation");
            options.AddArgument("--start-maximized");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--dns-prefetch-disable");
            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--incognito");
            options.AddArgument("--enable-precise-memory-info");
            options.AddArgument("--disable-default-apps");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-infobars");

            //if (ProgramConfig.IsChromeHeadless)
            //	options.AddArguments("--headless");

            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("profile.default_content_setting_values.automatic_downloads", 1);
            options.AddUserProfilePreference("browserstack.timezone", "Brazil");

            return options;
        }
    }
}