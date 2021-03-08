using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GithubUI
{
    class Homepage : TestsModel
    {

        [Test]

        public void CheckHomepage()
        {
            //this method checks if all images and links of homepage are working correctly 
            string tagName = null, attribute = null, description = null;


            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
        | SecurityProtocolType.Tls11
        | SecurityProtocolType.Tls12
        | SecurityProtocolType.Ssl3;

            List<string[]> errorCodes = new List<string[]>();

            //two iterations, the first referring to links and the second to images
            for (int iteration = 0; iteration < 2; iteration++)
            {

                if (iteration == 0)
                {
                    tagName = "a";
                    attribute = "href";
                    description = "links";

                }
                else
                {
                    tagName = "img";
                    attribute = "src";
                    description = "images";

                }
                driver.LogStep("Retrieving all " + description + " on the page");

                errorCodes = ElementValidation(tagName, attribute, description, errorCodes);

            }

            PrintErrorCodes(errorCodes);
        }

        public void PrintErrorCodes(List<string[]> errorCodes)
        {
            //this method prints all error codes found
            string errorTopic = null;
            if (errorCodes.Count > 0)
            {
                foreach (string[] errors in errorCodes)
                {
                    if (errorTopic == null || errorTopic != errors[0])
                    {
                        driver.LogStep("List of errors related to " + errors[0] + "\n");
                        errorTopic = errors[0];
                    }
                    driver.LogStep("Error found with " + errors[1] + ". Error code: " + errors[2]);
                }
                Assert.Fail(errorCodes.Count + " links or images returned an unexpected code");
            }
        }

        public List<string[]> ElementValidation(string tagName, string attribute, string description, List<string[]> errorCodes)
        {
            driver.LogStep("Starting elements validation of " + description);
            int responseCode = 0;

            //retrieving the href
            var LinkElements = driver.FindElements(By.TagName(tagName));

            int count = LinkElements.Count;

            for (int i = 0; i < count; i++)
            {

                responseCode = 0;
                var element = LinkElements[i];
                var elAttribute = element.GetAttribute(attribute);

                //ignore the anchor links without href attribute
                if (string.IsNullOrEmpty(elAttribute))
                    continue;

                try
                {

                    //Creating the HttpWebRequest
                    HttpWebRequest request = WebRequest.Create(elAttribute) as HttpWebRequest;
                    //Setting the Request method HEAD
                    request.Method = "HEAD";
                    //Getting the Web Response.
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                    responseCode = (int)response.StatusCode;

                    //driver.LogStep(elAttribute + ": HTTP Status Code: " + responseCode);

                    if (responseCode != 200 && responseCode != 999)
                    {
                        errorCodes.Add(new string[] { description, elAttribute, responseCode.ToString() });
                    }

                }
                catch (WebException ex)
                {
                    //sometimes a webexception was thrown, so this situation is dealt differently
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null)
                        {
                            responseCode = (int)response.StatusCode;
                            if (responseCode != 200 && responseCode != 999)
                            {
                                errorCodes.Add(new string[] { description, elAttribute, responseCode.ToString() });
                            }
                        }
                        else
                        {
                            driver.LogStep(elAttribute + ": No HHTP Status code available");
                        }
                    }
                    else
                    {
                        driver.LogStep(elAttribute + ": No HHTP Status code available");
                    }
                }
            }
            return errorCodes;
        }

        [Test]

        public void CheckSearchEngine()
        {
            //this test asserts if search engine is working correctly
            driver.SignIn(true);
            bool resultsReturned = true;
            string[] possibleSearch = new string[]
            {
                "%&/()=?", "%", "2520495835", "test", "test%#$&", " automated test ", "a"
            };
            driver.LogStep("Asserting if search engine is working as expected");


            foreach (string input in possibleSearch)
            {
                //entering each input
                IWebElement searchBar = driver.WaitClickJSE("//*[contains(@class, 'header-search-input')]");
                searchBar.Clear();

                //sedning input
                searchBar.SendKeys(input);

                //clicking enter
                searchBar.SendKeys(Keys.Enter);

                Thread.Sleep(1000);
                //retrieving search results
                string searchResult = driver.WaitFindElement("(//*[contains(@class, 'codesearch-results')]/*/*/*)[2]").GetAttribute("textContent");

                //asserting if the search was related to the input, by looking at the screen result message, between apostrophes
                try
                {
                    Assert.AreEqual(searchResult.Split(new string[] { "'" }, StringSplitOptions.None)[1], input, "String searched " + input + " is not the same as the one that page shows results");
                    resultsReturned = false;
                }
                catch (IndexOutOfRangeException)
                {
                    resultsReturned = true;
                    //in case there is no result message, there must be results
                    Assert.IsTrue(driver.IsElementPresent("//*[@class= 'repo-list']/*"), "Search engine for input=" + input + " didn't work as expected");
                }

                //the last input 'a' should retrieve results. As this condition may fail, the test will only be inconclusive
                if (possibleSearch.Last().Equals(input))
                {
                    if (!resultsReturned)
                    {
                        Assert.Inconclusive("Input '" + input + "' should return results to further test filters. Change this variable");
                    }

                    CheckSortEngine();

                    CheckFilterLanguages();
                }
            }

        }

        public void CheckSortEngine()
        {
            driver.LogStep("Checking sort engine");

            //click on sort options dropdown
            driver.WaitClickJSE("//*[contains(@class, 'codesearch-results')]//*[@aria-haspopup='menu']");

            for (int i = 1; i < driver.WaitFindElements("//*[@role='menuitemradio' ]").Count + 1; i++)
            {
                //clicking each sort option
                string href = driver.WaitFindElement("(//*[@role='menuitemradio' ])[" + i + "]").GetAttribute("href");
                driver.WaitClickJSE("(//*[@role='menuitemradio' ])[" + i + "]");

                //this much wait is to avoid Abuse detection mechanism
                Thread.Sleep(7000);
                string[] orderItem = new string[driver.WaitFindElements("(//*[@class= 'repo-list']/*)").Count];

                for (int j = 1; j < orderItem.Length + 1; j++)
                {
                    //parameter s in href of the sort option tells us what kind of parameter we're sorting (stars, updated, forks)
                    if (href.Contains("s=stars"))
                    {
                        try
                        {
                            //strings returned were getting a lot of spaces and paragraphs
                            orderItem[j - 1] = driver.FindElement(By.XPath("(//*[@aria-label='star' ]/..)[" + j + "]")).GetAttribute("textContent").Trim();
                            Regex.Replace(orderItem[j - 1], @"\s+", string.Empty);


                        }
                        catch (NoSuchElementException) { }

                    }
                    else
                                if (href.Contains("s=updated"))
                    {
                        orderItem[j - 1] = driver.WaitFindElement("(//*[@class= 'repo-list']/*)[" + j + "]//*[@class='no-wrap']").GetAttribute("datetime");
                    }

                    else if (href.Contains("s=forks"))
                    {
                        //this condition differs from the previous ones, because i only saw one way of retrieving the forks number: entering the element and checking this number. 
                        //but if we enter, and then go back, the list is refreshed, and so we cannot continue the comparison
                        //thus, the algorithm opens the link in another tab and then close it, returning to the original tab
                        Actions newTab = new Actions(driver);

                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView();", driver.WaitFindElement("(//*[@class= 'repo-list']/*)[" + j + "]//*[@class='v-align-middle']"));

                        newTab.KeyDown(Keys.Control).Click(driver.WaitFindElement("(//*[@class= 'repo-list']/*)[" + j + "]//*[@class='v-align-middle']")).KeyUp(Keys.Control).Build().Perform();
                        Thread.Sleep(2000);

                        driver.SwitchTo().Window(driver.WindowHandles[1]);

                        Thread.Sleep(1000);


                        orderItem[j - 1] = driver.WaitFindElement("(//*[@class= 'social-count'])[2]").GetAttribute("textContent").Trim();

                        driver.SwitchTo().Window(driver.WindowHandles[1]).Close();

                        Thread.Sleep(1000);

                        driver.SwitchTo().Window(driver.WindowHandles[0]);
                    }
                    try
                    {
                        //numbers were retrieved like '12.4k', so they must be turned into '12400'
                        if (orderItem[j - 1].Contains("k"))
                        {
                            orderItem[j - 1] = orderItem[j - 1].Replace("k", "");
                            decimal numberDecimal = decimal.Parse(orderItem[j - 1], CultureInfo.InvariantCulture);
                            long number = Convert.ToInt64(Convert.ToDouble(numberDecimal));
                            numberDecimal = numberDecimal * 1000;
                            orderItem[j - 1] = numberDecimal.ToString();
                            orderItem[j - 1] = orderItem[j - 1].Split(',')[0];
                        }
                    }
                    catch (NullReferenceException) { }


                }
                //parameter 'o' in href refered to ascending (asc) or descending (desc) order
                if (href.Contains("o=asc"))
                {
                    driver.CheckAlphabeticalOrder(orderItem);
                }
                else
                {
                    driver.CheckAlphabeticalOrder(orderItem, false);
                }


            }
        }

        public void CheckFilterLanguages()
        {
            string filtersPath = null, selectFilter = null;
            driver.LogStep("Checking if programming language filters are working as expected");

            for (int iteration = 0; iteration < 2; iteration++)
            {
                //iteration==0 -> programming languages; iteration==1 -> repositories
                if (iteration == 0)
                {
                    filtersPath = "//*[@class= 'filter-list small']/*";
                    selectFilter = "//*[@class= 'filter-item selected']";
                }
                else
                {
                    filtersPath = "//*[@class='menu-item']/*[contains(@class, 'codesearch')]";
                    selectFilter = "//*[@class='menu-item selected']/*[contains(@class, 'codesearch')]";

                }

                var filters = driver.WaitFindElements(filtersPath);

                for (int i = 1; i < filters.Count + 1; i++)
                {
                    //click each repository/language, and check if results all have this repository or language associated
                    if (iteration == 0)
                    {
                        driver.WaitClickJSE("(//*[@class= 'filter-list small']/*)[" + i + "]//*[@class='filter-item']");
                        string filterSelected = driver.WaitFindElement(selectFilter).GetAttribute("textContent").Trim();

                        Assert.IsTrue(driver.WaitFindElements("//*[@itemprop='programmingLanguage' and text()='" + filterSelected + "']").Count == driver.WaitFindElements("//*[@itemprop='programmingLanguage']").Count, "Results presented do not correspond to filter " + filterSelected);

                    }
                    else
                    {
                        driver.WaitClickJSE("(//*[@class='menu-item']/*[contains(@class, 'codesearch')])[" + i + "]/..");

                    }

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
