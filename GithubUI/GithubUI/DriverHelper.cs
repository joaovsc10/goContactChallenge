using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GithubUI
{
    public class DriverHelper : FirefoxDriver
    {
        /// <summary>
        /// This class is dedicated to common methods
        /// </summary>

        public WebDriverWait _wait;
        public DriverHelper(FirefoxOptions options) : base(options)
        {
            _wait = new WebDriverWait(this, TimeSpan.FromMinutes(3));
        }
        public void LogStepDetailed(string log)
        {
            Console.Error.WriteLine(DateTime.Now.ToString("h:mm:ss tt") + "->" + log);
        }
        #region LogMethods
        public void LogStep(string log)
        {
            TestContext.WriteLine("->" + log);
            LogStepDetailed(log);
        }
        #endregion


        #region WebDriverExtendedMethods



        /// <summary>
        /// Clicks IWebElement executing JavaScript. 
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>                               
        public IWebElement WaitClickJSE(string xpath)
        {
            IsElementPresent(xpath);
            //string loadingEl = "//*[@class='placeholder disable-pointer-events' and contains(@style,'display: none')]";

            _wait.Until(x => x.FindElement(By.XPath(xpath)).Enabled);
            IWebElement el = this.FindElement(By.XPath(xpath));

            // Add click handler to button to check when button was clicked
            String jsButtonHandler = @"window.elementIsClicked = false; 
                                       function clickHandlerUXTests() { 
                                        window.elementIsClicked = true;
                                       }
                                       var element = arguments[0]; 
                                       element.addEventListener('click', clickHandlerUXTests);";

            ((IJavaScriptExecutor)this).ExecuteScript(jsButtonHandler, el);

            // Try to click element until defined timeout
            ((IJavaScriptExecutor)this).ExecuteScript("arguments[0].focus(); arguments[0].click()", el);

            return el;
        }
        /// <summary>
        /// Clicks IWebElement executing JavaScript. 
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public IWebElement WaitClickJSE(IWebElement el, string href = null)
        {
            string previousUrl = this.Url;


            // Add click handler to button to check when button was clicked
            String jsButtonHandler = @"window.elementIsClicked = false; 
                                       function clickHandlerUXTests() { 
                                        window.elementIsClicked = true;
                                       }
                                       var element = arguments[0]; 
                                       element.addEventListener('click', clickHandlerUXTests);";

            ((IJavaScriptExecutor)this).ExecuteScript(jsButtonHandler, el);


            bool IsButtonClicked = false;
            long tickStart = DateTime.Now.Ticks;
            while (!IsButtonClicked)
            {
                // Try to click element until defined timeout
                ((IJavaScriptExecutor)this).ExecuteScript("arguments[0].focus(); arguments[0].click()", el);

                // Check status of button
                IsButtonClicked = (bool)((IJavaScriptExecutor)this).ExecuteScript("return window.elementIsClicked;");
                //&& (!IsElementPresent(loadingEl));
                // (waitUntilButtonIsSelected? CheckIfButtonStatusIsSelected(el): true);// && CheckIfButtonStatusIsSelected(el);
                // && ((String)((IJavaScriptExecutor)this).ExecuteScript("return $(arguments[0]).prop('class')", el)).Contains("selected"));

                if (IsButtonClicked)
                {
                    System.Threading.Thread.Sleep(500);
                }

                if ((DateTime.Now.Ticks - tickStart) > _wait.Timeout.Ticks)
                {
                    Assert.Fail($"Timeout expired and element was not clicked.");
                }
            }
            if (href != null)
            {
                try
                {
                    new WebDriverWait(this, TimeSpan.FromSeconds(60)).Until(d => previousUrl.Equals(this.Url) == false);

                }
                catch (WebDriverTimeoutException)
                {
                    Assert.Inconclusive("Page " + href + " took too much time to load.");
                }

                Assert.AreEqual(this.Url, href, "The URL of the page redirected by the button with href=" + href + "  is not the same as expected");


            }
            //((IJavaScriptExecutor)this).ExecuteScript("window.elementIsClicked = false");
            return el;
        }

        /// <summary>
        /// Check if element is present before returning it. 
        /// </summary>
        public IWebElement WaitFindElement(string xpath)
        {
            IsElementPresent(xpath);
            IWebElement elem = FindElement(By.XPath(xpath));

            return elem;
        }

        /// <summary>
        /// Check if elements are present before they are returned. 
        /// </summary>
        public ReadOnlyCollection<IWebElement> WaitFindElements(string xpath)
        {
            IsElementPresent(xpath);
            var elems = FindElements(By.XPath(xpath));

            return elems;
        }


        #endregion

        /* Other ***********************************************************************************************/


        /// <summary>
        /// Check if element (xpath) is available at page. 
        /// If element is found before specified timeSpan returns true, else returns false.  
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="timeSpan">Timeout in seconds.</param>
        /// <returns></returns>
        public bool IsElementPresent(string xpath, int timeSpan = 5)
        {
            try
            {
                new WebDriverWait(this, TimeSpan.FromSeconds(timeSpan)).Until(x => x.FindElement(By.XPath(xpath)));
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        /// <summary>
        /// Check if array (names) is in the correct alphabetical order. 
        /// </summary>
        public void CheckAlphabeticalOrder(string[] names, bool ascendingOrder = true)
        {
            for (int i = 0; i < names.Length - 1; i++)
            {
                if (ascendingOrder)
                {
                    if (StringComparer.OrdinalIgnoreCase.Compare(names[i], names[i + 1]) > 0)
                    {
                        Assert.Fail("Names are incorrectly sorted in an ascending order. Error found in " + names[i] + " and " + names[i + 1]);
                        break;
                    }
                }
                else
                {
                    if (StringComparer.OrdinalIgnoreCase.Compare(names[i], names[i + 1]) < 0)
                    {
                        Assert.Fail("Names are incorrectly sorted in an descending order. Error found in " + names[i] + " and " + names[i + 1]);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Signs in. IF the bool enter is false, the method tries several combinations
        /// </summary>
        public void SignIn(bool enter = false)
        {
            int iteration = 0;
            if (enter)
            {
                LogStep("Signing in...");
                iteration = 2;
            }
            IWebElement inputElement = null;
            // list of username and password combinations.
            List<string[]> signInData = new List<string[]>() {
   new string[] {"login", "testegocontact1@gmail.com","testegocontact10@gmail.com", "testegocontact10@gmail.com" },
   new string[] {"password", "testegocontact10", "testegocontact11", "testegocontact10" }
 };
            //click on sign-in
            WaitClickJSE("//a[@href='/login']");
            Thread.Sleep(5000);

            for (int j = iteration; j < 3; j++)
            {

                foreach (var array in signInData)
                {
                    //click on the input field
                    inputElement = WaitClickJSE("//*[@name='" + array[0] + "' and contains(@class,'input')]");

                    //clear and send data
                    inputElement.Clear();
                    inputElement.SendKeys(array[j + 1]);

                }

                Thread.Sleep(1000);

                inputElement.SendKeys(Keys.Enter);

                //only the third iteration should make a successful login
                if (!IsElementPresent("//*[contains(@class,'logged-in')]") && j == 2 || (IsElementPresent("//*[contains(@class,'logged-in')]") && j != 2))
                {
                    Assert.Fail("Log in not working properly.");
                }

            }

        }

        /// <summary>
        /// Check if stars in each element can be added when the user is logged, and if when the are clicked, they redirect to login page when no user is logged
        /// string [] titles refers to element titles
        /// </summary>
        public string[] CheckStars(string[] titles = null, bool checkTitles = false)
        {
            string href = null;
            var stars = WaitFindElements("//*[contains(@class,'octicon-star')]/..");
            for (int starNumber = 1; starNumber < stars.Count; starNumber++)
            {
                href = WaitFindElement("((//*[contains(@class,'octicon-star')])[" + starNumber + "]/../../*)[1]").GetAttribute("href");

                if (IsElementPresent("//*[contains(@class,'logged-in')]"))
                {
                    //Check if stars in each element cannot be added when the user is not logged in
                    Assert.IsTrue(!WaitFindElement("(//*[contains(@class,'octicon-star')])[" + starNumber + "]/..").GetAttribute("href").Contains("login"), "Topic with href=" + FindElement(By.XPath("(//*[contains(@class,'octicon-star')])[" + starNumber + "]/../..")).GetAttribute("href") + " can be added without login");
                }
                else
                {
                    //Check if stars in each element can be added when the user is logged
                    Assert.IsTrue(FindElement(By.XPath("(//*[contains(@class,'octicon-star')])[" + starNumber + "]/..")).GetAttribute("href").Contains("login"), "Topic with href=" + FindElement(By.XPath("(//*[contains(@class,'octicon-star')])[" + starNumber + "]/../..")).GetAttribute("href") + " cannot be added with login");

                }
                if (checkTitles)
                {
                    titles[starNumber - 1] = FindElement(By.XPath("(//*[contains(@class,'octicon-star')])[" + starNumber + "]/../../..//*[contains(@class, 'Link--primary')]")).GetAttribute("textContent");
                }

            }

            return titles;
        }

        /// <summary>
        /// Given an href, the method opens profile dropdown, and click the item that contains this href
        /// </summary>
        public void AccessProfileDropdownItem(string href)
        {
            Thread.Sleep(1000);
            //click on profile dropdown
            WaitClickJSE("//*[@class='Header-link' and contains(@aria-label, 'profile')]");

            LogStep("Clicking on " + href + "...");

            WaitClickJSE("//*[@class='dropdown-item' and contains(@href, '" + href + "')]");

            Thread.Sleep(1000);
        }

    }


}

