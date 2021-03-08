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
    class Repository : TestsModel
    {
        [Test]
        public void CheckRepoOperations()
        {
            //this test adds, edits and delete a repository
            string repositoryName = "test";

            //add repository
            AddRepo(repositoryName);

            //edit repository
            repositoryName = EditRepositoryName(repositoryName);

            //delete repository
            DeleteRepository(repositoryName);


        }
        public void AddRepo(string repositoryName, bool isRepository = true)
        {
            string id = "repository", dropDownItem = "repositories";


            driver.SignIn(true);

            driver.LogStep("Clicking on profile dropdown...");

            for (int i = 0; i < 2; i++)
            {
                driver.AccessProfileDropdownItem(dropDownItem);

                //clicking on new button
                driver.WaitClickJSE("//*[contains(@href,'/new') and contains(@class, 'btn btn-primary')]");

                //clicking on the name
                IWebElement name = driver.WaitFindElement("//*[@id='" + id + "_name']");

                name.SendKeys(repositoryName);

                if (i == 0)
                {
                    //checking if there is another repository wih the same name
                    Assert.IsTrue(driver.IsElementPresent("//*[@id='repository_name' and contains(@class, 'autocheck-successful')]"), "There is a repository with the name 'test'");

                    //click on create repository/project
                    driver.WaitClickJSE("//*[contains(@class, 'primary') and @type='submit']");
                    Thread.Sleep(3000);
                }
                else
                {
                    driver.LogStep("Checking if the system doesn't accept repeated repository names and if it keeps the create button disabled");
                    Assert.IsTrue(driver.IsElementPresent("//*[@id='repository_name' and contains(@class, 'autocheck-errored')]"), "It should not be possible to create two repositories with the same name");
                    Assert.IsTrue(driver.IsElementPresent("//*[contains(@class, 'primary') and @type='submit' and @disabled='']"), "The button Create Repository should not be enabled when the name of the repository is repeated");


                }
            }

        }

        public string EditRepositoryName(string repositoryName)
        {
            for (int i = 0; i < 2; i++)
            {
                driver.AccessProfileDropdownItem("repositories");

                //checking if the list of repositories has the same number has the number presented under Repositories title
                Assert.IsTrue(driver.WaitFindElements("//*[@itemprop='owns']").Count.ToString() == driver.WaitFindElement("(//*[contains(@href,'repositories')]//*[contains(@class,'Counter')])[1]").GetAttribute("textContent").Trim());

                //checking if href is correct
                Assert.IsTrue(driver.IsElementPresent("//*[@href='/testgocontact/" + repositoryName + "']"), "The href for the repository " + repositoryName + " is not correct");

                driver.WaitClickJSE("//*[@href='/testgocontact/" + repositoryName + "']");

                if (i == 0)
                {
                    //clicking on settings

                    driver.WaitClickJSE("//*[@data-content='Settings']");

                    //check if the rename doesn't work with the same name
                    driver.WaitClickJSE("//*[@type='submit' and @class='btn flex-self-end']");
                    Assert.IsTrue(driver.IsElementPresent("//*[contains(@class, 'error')]"), "The system is accepting the same name to rename the repository");

                    Thread.Sleep(2000);
                    //clicking on rename field, clear and send input
                    IWebElement rename = driver.WaitClickJSE("//*[@id='rename-field']");
                    repositoryName = "automatedTest";
                    rename.Clear();
                    rename.SendKeys(repositoryName);
                    Thread.Sleep(2000);

                    //checking if there is no repository with the same name
                    Assert.IsTrue(driver.IsElementPresent("//*[@id='rename-field' and contains(@class, 'autocheck-successful')]"), "There is a repository with the name '" + repositoryName + "'");
                    driver.WaitClickJSE("//*[@type='submit' and @class='btn flex-self-end']");

                    //checking if href is updated
                    Assert.IsTrue(driver.IsElementPresent("//*[@href='/testgocontact/" + repositoryName + "']"), "Renaming to " + repositoryName + " was not successful");
                }
            }
            return repositoryName;
        }

        public void DeleteRepository(string repositoryName)
        {
            driver.WaitClickJSE("//*[@href='/testgocontact/" + repositoryName + "']");

            //clicking on settings
            driver.WaitClickJSE("//*[@data-content='Settings']");

            //clicking on delete
            driver.WaitClickJSE("(//*[contains(@class, 'btn-danger boxed-action')])[4]");


            IWebElement verifier = driver.WaitClickJSE("(//*[@name='verify'])[3]");

            //write delete confirmation
            verifier.SendKeys("testgocontact/" + repositoryName);

            driver.WaitClickJSE("(//*[contains(@class, 'danger') and @type='submit' and @data-disable-invalid=''])[4]");

            driver.AccessProfileDropdownItem("repositories");

            //check if it was deleted
            Assert.IsTrue(!driver.IsElementPresent("//*[@href='/testgocontact/" + repositoryName + "']"), "The href for the repository " + repositoryName + " was not deleted");

        }





    }
}
