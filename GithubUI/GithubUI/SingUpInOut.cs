
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GithubUI
{
    [TestFixture]
    class SingUpInOut : TestsModel
    {

        [Test]
        public void SignUp()
        {
            driver.LogStep("Signing up...");

            bool clearTextBox = false;
            string text = "";
            driver.LogStep($"arguments[0].scrollIntoView(false); arguments[0].focus(); {(clearTextBox ? "$(arguments).val(''); " : "")} $(arguments).sendkeys('{text}');");
            IWebElement inputElement;

            List<string[]> signUpData = new List<string[]>() {
   new string[] {"email","a", "abc@", "testegocontact10@gmail.com", "abc@" },
   new string[] {"password","abcdefghijklmno", "pqazuwsxçedsqw", "1qweasq", "pqazuwsx", "1QASWEDQ"}
 };

            //click on sign-up
            driver.WaitClickJSE("//*[@action='/join']//*[@type='submit']");
            Thread.Sleep(5000);

            //click on email field
            Assert.AreEqual(driver.Url, "https://github.com/join", "The current URL does not match the expected one");

            foreach (var array in signUpData)
            {
                inputElement = driver.WaitClickJSE("//*[@name='user[" + array[0] + "]' and contains(@class,'input')]");

                for (int i = 1; i < array.Length; i++)
                {
                    //sends each input
                    inputElement.SendKeys(array[i]);
                    Thread.Sleep(1000);
                    try
                    {
                        driver.WaitFindElement("//*[@name='user[" + array[0] + "]' and contains(@class, 'errored')]");
                    }
                    catch (NoSuchElementException)
                    {
                        Assert.Fail(array[0] + " field in Sign Up is accepting '" + array[i] + "' as an input.");
                    }
                    inputElement.Clear();
                }
                Thread.Sleep(1000);

            }
        }

        [Test]
        public void SignInSignOut()
        {
            driver.SignIn();
            SignOut();
        }




        public void SignOut()
        {
            //click profile dropdown
            driver.WaitClickJSE("//*[@class='Header-link' and contains(@data-ga-click, 'avatar')]");

            //click signout
            driver.WaitClickJSE("//*[contains(@class, 'signout')]");

            if (!driver.IsElementPresent("//*[contains(@class,'logged-out')]"))
            {
                Assert.Fail("Log out not working properly.");
            }
        }


    }
}
