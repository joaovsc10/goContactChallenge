using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;

namespace GithubUI
{
    public abstract class TestsModel
    {
        //this class has all essential methods when executing a test

        public DriverHelper driver;
        FirefoxOptions options;
        private string url = "";

        [OneTimeSetUp]
        public void initialSetup()
        {
            url = "https://github.com";
            options = new FirefoxOptions();
        }
        [SetUp]
        public void setUp()
        {
            //maximizes window and goes to url
            driver = new DriverHelper(options);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(url);

        }
        [TearDown]
        protected void TestCleanup()
        {
            // Close IWebDriver
            if (!(driver.SessionId == null))
            {
                driver.Close();
                driver.Dispose();
            }

        }

        [OneTimeTearDown]
        protected void TerminateDriver()
        {
            // Close IWebDriver
            driver.Close();
            driver.Dispose();

        }


    }
}
