using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GithubUI
{
    class Settings : TestsModel
    {
        [Test]
        public void CheckAppearance()
        {
            //this test checks if the different color modes and emojis preferences are correctly saved
            driver.SignIn(true);

            driver.AccessProfileDropdownItem("settings");

            driver.WaitClickJSE("//*[contains(@class,'js-selected-navigation-item') and contains(@href, '/settings/appearance')]");

            CheckColorMode();

            CheckEmojiPreference();

        }

        public void CheckEmojiPreference()
        {
            driver.LogStep("Checking if all emoji preference choices can be saved");
            //retrieving all emoji preferences

            var emojis = driver.WaitFindElements("//*[@name='user[emoji_skin_tone_preference]']");

            for (int i = 1; i < emojis.Count + 1; i++)
            {
                //click on each emoji
                driver.WaitClickJSE("(//*[@name='user[emoji_skin_tone_preference]'])[" + i + "]");

                //update preference
                driver.WaitClickJSE("//*[@aria-labelledby='emoji-heading']//*[@type='submit']");

                //check if change was saved
                Assert.IsTrue(driver.IsElementPresent("(//*[@name='user[emoji_skin_tone_preference]'])[" + i + "][@checked='']"), "Emoji preference number " + i + " didn't get saved");

            }
        }

        public void CheckColorMode()
        {
            driver.LogStep("Asserting if all color modes are working");
            var colorModes = driver.WaitFindElements("//*[@aria-labelledby='color-mode-heading']//*[@name='user_theme']");

            foreach (IWebElement colorMode in colorModes)
            {
                driver.WaitClickJSE(colorMode);

                string dataColorMode = colorMode.GetAttribute("data-mode");
                try
                {
                    driver.WaitFindElement("//*[@data-color-mode='" + dataColorMode + "']");
                }
                catch (NoSuchElementException)
                {
                    Assert.Fail("Data color mode " + dataColorMode + " is not working");
                }
            }
        }

        [Test]
        public void ProfileData()
        {
            //this test tries different inputs on profile data and checks if they were correctly saved

            driver.SignIn(true);
            driver.AccessProfileDropdownItem("settings");

            //Try different data in each filter to check if one field may have the value of another
            string[,] synonyms = new string[6, 7]
            {
                {"user_profile_name","//*[@itemprop='name']",  "%&/()=?", "2520495831", "test", "test%#$&", " automated test "},

                {"user_profile_bio","//*[contains(@class,'p-note user-profile-bio')]",  "%&/()=?#", "2520495832", "testa", "testa%#$&", " automated testa "},

                {"user_profile_blog","//*[@itemprop='url']//*[contains(@class, 'Link--primary')]",  "%&/()=?-", "2520495833", "testb", "testb%#$&", "automated testb"},

                {"user_profile_twitter_username","//*[@itemprop='twitter']//*[contains(@class, 'Link--primary')]",  "___", "2520495834", "testc", "testc_", "automatedtestc"},

                {"user_profile_company", "//*[@itemprop='worksFor']//*[@class='p-org']",  "%&/()=?»", "2520495835", "testd", "testd%#$&", " automated testd "},

                {"user_profile_location","//*[@itemprop='homeLocation']//*[@class='p-label']",  "%&/()=?«", "2520495836", "teste", "teste%#$&", " automated teste "},
            };
            //iterates through all different inputs
            for (int j = 2; j < synonyms.GetLength(1); j++)
            {
                //iterates through all kinds of inputs
                for (int i = 0; i < synonyms.GetLength(0); i++)
                {
                    IWebElement inputField = driver.WaitClickJSE("//*[@id='" + synonyms[i, 0] + "']");
                    //clears input
                    inputField.Clear();

                    //sends input
                    inputField.SendKeys(synonyms[i, j]);
                    Thread.Sleep(1000);

                }

                //click on update button

                driver.WaitClickJSE("//*[@class='edit_user']//*[@class= 'btn btn-primary']");

                Thread.Sleep(1000);

                driver.Navigate().Refresh();

                //click to go to personal profile

                driver.WaitClickJSE("//*[@href='/testgocontact' and @class='btn btn-sm']");

                driver.LogStep("Asserting if the information is correctly placed");
                for (int l = 0; l < synonyms.GetLength(0); l++)
                {
                    if (synonyms[l, 0].Equals("user_profile_twitter_username"))
                    {
                        Assert.AreEqual(driver.WaitFindElement(synonyms[l, 1]).GetAttribute("textContent").Trim(), "@" + synonyms[l, j]);

                    }
                    else
                    {
                        try
                        {
                            Assert.AreEqual(driver.WaitFindElement(synonyms[l, 1]).GetAttribute("textContent"), synonyms[l, j]);
                        }
                        catch (NoSuchElementException)
                        {
                            Assert.AreEqual(driver.WaitFindElement("//*[@itemprop='url']").GetAttribute("textContent").Trim(), synonyms[l, j]);

                        }
                    }
                }

                driver.Navigate().Back();
            }



        }
    }
}
