using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubUI
{
    class PricingPlan : TestsModel
    {
        [Test]
        public void CheckPricingPlan()
        {
            string logoName = null;
            driver.LogStep("Going to https://github.com/pricing");
            driver.Navigate().GoToUrl("https://github.com/pricing");

            driver.LogStep("Retrieving the testimonial logos");
            var testimonialLogo = driver.FindElements(By.XPath("//*[contains(@class, 'pricing-testimonial-logo') and not (contains(@class, 'rounded'))]"));

            driver.LogStep("Asserting if the testimonial logos and the text of the testimonial belong to the same enterprise");
            foreach (IWebElement logo in testimonialLogo)
            {
                driver.WaitClickJSE(logo);

                logoName = logo.FindElement(By.XPath("./*/*[@loading='lazy']")).GetAttribute("alt");

                Assert.IsTrue(driver.FindElement(By.XPath("//*[contains(@class, 'js-pricing-testimonial-quote pricing-testimonial-content pricing-testimonial-content-active')]")).GetAttribute("textContent").Contains(logoName), "Testimonial from " + logoName + "doesn't belong to them");
            }

            driver.LogStep("All testimonial belongs to the correct enterprise!");
        }

    }
}
