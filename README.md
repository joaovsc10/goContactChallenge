Instruction to run the tests

-Make sure that you have Visual Studio installed. This work was developed under version from 2015, because the execution is more fast and stable;
-To directly open the whole project, open "GithubUI\GithubUI.sln"
- Install the last stable versions of the following NuGet packages: NUnit, NUnit3TestAdapter and Selenium.WebDriver (Right click on solution -> Manage NuGet Packages... -> Browse)
- Make sure you have Firefox (this work was developed under version 77.0.1)

-The Solution Explorer tab shows the classes that are implemented in this project. Click right button on the solution (GithubUI) -> Build Solution
- The Test Explorer tab shows all tests, that can be runned individually, by selecting the test -> right click -> Run Selected Tests
- You can also run all, clicking the underlined option in Test Explorer tab "Run All"
- If executing the test directly on Visual Studio, instead of Jenkins, don't click or move the mouse through any element of the tab that the test is being executed.

- All .dll files and drivers are in "..\GithubUI\GithubUI\bin\Debug"
