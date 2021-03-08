using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GithubUI
{
    class TopicsTrendingsPage : TestsModel
    {
        [Test]
        public void CheckTopicsAndTrendings()
        {
            string tagName = null;
            IWebElement tag = null;
            driver.LogStep("Going to https://github.com/topics");
            driver.Navigate().GoToUrl("https://github.com/topics");

            driver.LogStep("Retrieving all popular topics elements");
            var topicTags = driver.WaitFindElements("//*[contains(@class,'topic-tag')]");

            driver.LogStep("Checking if popular topic name is the same as the title presented in the redirected page");
            for (int i = 1; i < topicTags.Count + 1; i++)
            {
                tag = driver.WaitFindElement("(//*[contains(@class,'topic-tag')])[" + i + "]");

                driver.WaitClickJSE(tag);

                tagName = tag.GetAttribute("textContent").ToString().ToLower().Trim();

                driver.IsElementPresent("//*[@class='h1-mktg']");

                //checking if popular topic name matches title
                Assert.AreEqual(tagName, driver.FindElement(By.XPath("//*[@class='h1-mktg']")).GetAttribute("textContent").ToLower().Trim().Replace(".", ""), "Popular topic " + tagName + " doesn't match the page title");

                //returning
                driver.Navigate().Back();

                topicTags = driver.WaitFindElements("//*[contains(@class,'topic-tag')]");

                //check if when clicking a star without login redirects to login page

                var stars = driver.WaitFindElements("//*[contains(@class,'octicon-star')]/..");
                string[] titles = new string[stars.Count];
                driver.CheckStars(titles, true);

                driver.CheckAlphabeticalOrder(titles);
            }
        }

        [Test]
        public void CheckTrending()
        {
            driver.SignIn(true);
            driver.LogStep("Going to https://github.com/trending");
            driver.Navigate().GoToUrl("https://github.com/trending");

            CheckLanguagesAndDateFilter();
        }

        public void CheckLanguagesAndDateFilter()
        {
            //this method checks if languages (iteration=0) and date (iteration=1) filters are working properly
            string menu = null, resultElementPath;

            for (int iteration = 0; iteration < 1; iteration++)
            {
                if (iteration == 0)
                {
                    menu = "language";
                    resultElementPath = "@itemprop = 'programmingLanguage'";
                }
                else
                {
                    menu = "date";
                    resultElementPath = "@class='d-inline-block float-sm-right'";
                }
                int counter = 0;
                driver.LogStep("Assert if " + menu + " filter is working...");

                driver.LogStep("Clicking " + menu + " filter dropdown...");
                driver.WaitClickJSE("//*[@id='select-menu-" + menu + "']");
                var dropdownItems = driver.WaitFindElements("//*[@id='select-menu-" + menu + "']//*[@role='menuitemradio']");

                for (int i = 1; i < dropdownItems.Count + 1; i++)
                {
                    //the second condition had to be implemented, because there was 2 item with the same name next to each other, and when one was selected, and the system clicks the other with the same name, the filter assumes that I meant to show any languages
                    if (i == 1 || !dropdownItems[i - 1].GetAttribute("textContent").Trim().Equals(dropdownItems[i - 2].GetAttribute("textContent").Trim()))
                    {
                        //clicking option
                        driver.WaitClickJSE("(//*[@id='select-menu-" + menu + "']//*[@role='menuitemradio'])[" + i + "]");
                        driver.IsElementPresent("//*[" + resultElementPath + "and text()[contains(.,'" + dropdownItems[i - 1].GetAttribute("textContent") + "')]]");

                        dropdownItems = driver.WaitFindElements("//*[@id='select-menu-" + menu + "']//*[@role='menuitemradio']");

                        //theres a limitation in xpath 1.0, that prevents an apostrophe (') from being used in a path. In order to avoid an InvalidSelectorException, backslash must be added in the beggining and the end of the string, and contains() method must be used
                        //retrieving number of results
                        if (iteration == 1)
                        {
                            counter = driver.WaitFindElements("//*[" + resultElementPath + "and text()[contains(.,'" + dropdownItems[i - 1].GetAttribute("textContent").Trim().ToLower() + "')]]").Count;
                        }
                        else
                        {
                            try
                            {

                                counter = driver.WaitFindElements("//*[" + resultElementPath + " and text()='" + dropdownItems[i - 1].GetAttribute("textContent").Trim() + "']").Count;
                            }
                            catch (InvalidSelectorException)
                            {
                                counter = driver.WaitFindElements("//*[" + resultElementPath + " and contains(text(),\"" + dropdownItems[i - 1].GetAttribute("textContent").Trim() + "\")]").Count;

                            }

                        }

                        //checking if the number of results matches the expected
                        var results = driver.FindElements(By.XPath("//*[" + resultElementPath + "]"));
                        if (results.Count != counter)
                        {
                            for (int j = 0; j < results.Count; j++)
                            {
                                Assert.IsTrue(dropdownItems[i - 1].GetAttribute("textContent").Trim().Contains(results[0].GetAttribute("textContent")), "Error found in " + menu + " " + dropdownItems[i - 1].GetAttribute("textContent").Trim());
                            }

                        }

                    }
                    driver.CheckStars();
                    driver.WaitClickJSE("//*[@id='select-menu-" + menu + "']");
                }

            }
            CheckAddStar();
        }

        public void CheckAddStar()
        {
            driver.LogStep("Check if by clicking on the star to add an element, if it goes to the list of starred elements...");

            driver.LogStep("Clearing language");
            //clicking language dropdown
            driver.WaitClickJSE("//*[@id='select-menu-language']/*");

            //clicking to clear language selection, to ensure that at least one result is presented on screen
            driver.WaitClickJSE("//*[contains(@class,'select-menu-clear-item')]/*[@role='menuitem']");

            //click on the star of the first element
            driver.WaitClickJSE("(//*[contains(@class,'unstarred js-social-form')])[1]/*[@type='submit']");


            string firstElementName = driver.WaitFindElement("(//*[@class='h3 lh-condensed'])[1]/*").GetAttribute("textContent");

            //removing all spaces in string
            firstElementName = Regex.Replace(firstElementName, @"\s+", string.Empty);

            driver.LogStep("Clicking on profile dropdown...");
            //click on profile dropdown
            driver.WaitClickJSE("//*[@class='Header-link' and contains(@aria-label, 'profile')]");

            driver.LogStep("Clicking on Your stars...");
            //click on "Your stars"
            driver.WaitClickJSE("//*[@class='dropdown-item' and contains(@href, 'stars')]");

            //find element that should be in 'Your Stars'
            try
            {
                driver.WaitFindElement("//*[@href='/" + firstElementName + "']");
            }
            catch (NoSuchElementException)
            {
                Assert.Fail("Element " + firstElementName + " is not in Your Stars menu");
            }

            driver.LogStep("Clicking on Unstar...");
            driver.WaitClickJSE("//*[@value='Unstar']");
            driver.Navigate().Refresh();
            Thread.Sleep(1000);

            driver.LogStep("Checking if element got removed");
            try
            {
                driver.WaitFindElement("//*[@href='/" + firstElementName + "']");
                Assert.Fail("Element " + firstElementName + " did not get removed from Your Stars menu");
            }
            catch (NoSuchElementException)
            {
                Assert.Pass();
            }


        }

    }
}
