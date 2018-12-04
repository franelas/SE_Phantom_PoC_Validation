/*************************************************************************************************************************************\
|                                                             LIBRARIES                                                               |
\*************************************************************************************************************************************/

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SE_Phantom_PoC_Validation.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SE_Phantom_PoC_Validation
{
    class WebDriver
    {
        /*****************************************************************************************************************************\
        |                                                      PRIVATE VARIABLES                                                      |
        \*****************************************************************************************************************************/
        private IWebDriver driver;

        private String url;
        

        /*****************************************************************************************************************************\
        |                                                      PRIVATE FUNCTIONS                                                      |
        \*****************************************************************************************************************************/        
        private IWebElement customScrollToElementById(String id)
        {
            var element = driver.FindElement(By.Id(id));
            scrollToView(element);
            return element;
        }

        private void scrollToView(IWebElement element)
        {
            if (element.Location.Y > 200)
            {
                scrollTo(element.Location.X, element.Location.Y - 100); // Make sure element is in the view but below the top navigation pane
            }

        }

        private void typeTextIntoField (FormDatum datum, String iframe, bool safeType)
        {
            int numRetries = 0;
            IWebElement detailFrame;
            bool typed = false;
            driver.SwitchTo().DefaultContent();
            if(iframe != null && iframe != "")
            {
                new WebDriverWait(driver, TimeSpan.FromSeconds(15)).Until(ExpectedConditions.ElementExists((By.XPath("//iframe[@id='" + iframe + "']"))));
                detailFrame = driver.FindElement(By.XPath("//iframe[@id='" + iframe + "']"));
                driver.SwitchTo().Frame(detailFrame);
            }

            //Find the element, which must be reachable by keyboard+
            while (!typed)
            {
                try
                {
                    switch (datum.fieldType)
                    {
                        case Enums.Form_FieldType.Text: typed = fillTextField(datum, iframe, safeType); break;
                        case Enums.Form_FieldType.Select: typed = fillSelectField(datum); break;
                        case Enums.Form_FieldType.Date: typed = fillDateField(datum); break;
                        case Enums.Form_FieldType.Autocomplete: typed = fillAutocompleteField(datum); break;
                        case Enums.Form_FieldType.Checkbox: typed = fillCheckBox(datum, iframe); break;
                        case Enums.Form_FieldType.Radio: typed = fillRadioButton(datum, iframe);break;
                        case Enums.Form_FieldType.WaitText: typed = fillWaitText(datum, iframe); break;
                        default: break;
                    }
                }
                catch (Exception ex)
                {
                    if (numRetries > 11)
                    {
                        Controller.Messages = Controller.Messages + datum.FieldId + " Not filled. ";
                        throw new Exception("Too much retries trying to reach the element with Id: " + datum.FieldId + ", Exception message: "+ex.Message );
                    }
                    numRetries++;
                    Thread.Sleep(500);
                    //reload iframe
                    driver.SwitchTo().DefaultContent();
                    if (iframe != null && iframe != "")
                    {
                        new WebDriverWait(driver, TimeSpan.FromSeconds(15)).Until(ExpectedConditions.ElementExists((By.XPath("//iframe[@id='" + iframe + "']"))));
                        detailFrame = driver.FindElement(By.XPath("//iframe[@id='" + iframe + "']"));
                        driver.SwitchTo().Frame(detailFrame);
                    }
                }
            }
        }

        private bool fillRadioButton(FormDatum datum, String iframe)
        {
            if (datum.FieldId == Constants.AddFederalTaxes.WithHoldRadioButton)
            {
                switch (datum.FieldValue)
                {
                    case Constants.AddFederalTaxes.Single_Value_WithHold: datum.FieldId = Constants.AddFederalTaxes.SingleRadioButton; break;
                    case Constants.AddFederalTaxes.Married_Value_WithHold: datum.FieldId = Constants.AddFederalTaxes.MarriedRadioButton; break;
                    case Constants.AddFederalTaxes.MarriedBut_Value_WithHold: datum.FieldId = Constants.AddFederalTaxes.MarriedButWithholdRadioButton; break;
                    default: throw new Exception("Unknown Withholding value for radio button");
                }
            }
            IWebElement radioElement = getIwebElement(datum.FieldId, iframe);
            radioElement.Click();
            return true;
        }

        private bool fillCheckBox(FormDatum datum, String iframe)
        {
            IWebElement boxElement = getIwebElement(datum.FieldId, iframe);
            if (datum.FieldValue == "True")
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                var query = String.Format("$('#"+datum.FieldId+"').click()");
                js.ExecuteScript(query);

                return true;
            }
            return false;
        }

        private bool fillSelectField(FormDatum datum)
        {
            bool typed = false;
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            var jsCommand = String.Format("document.getElementById('" + datum.FieldId + "').removeAttribute('style');");

            js.ExecuteScript(jsCommand);
            SelectElement fieldForm = new SelectElement(driver.FindElement(By.Id(datum.FieldId)));
            new WebDriverWait(driver, TimeSpan.FromMilliseconds(500)).Until(ExpectedConditions.ElementExists(By.Id(datum.FieldId)));

            fieldForm.SelectByText(datum.FieldValue);
            typed = true;
            return typed;
        }
        
        private bool fillDateField(FormDatum datum)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            var jsCommand = String.Format("$('#"+datum.FieldId+"').val('"+datum.FieldValue+"');");
            js.ExecuteScript(jsCommand);
            jsCommand = String.Format("$('#"+datum.FieldId+"').blur().focus();");
            js.ExecuteScript(jsCommand);
            return true;
        }

        private bool fillAutocompleteField (FormDatum datum)
        {
            IWebElement fieldForm = driver.FindElement(By.Id(datum.FieldId));
            new WebDriverWait(driver, TimeSpan.FromMilliseconds(500)).Until(ExpectedConditions.ElementIsVisible(By.Id(datum.FieldId)));
            if (fieldForm.Enabled)
            {
                fieldForm.Clear();
                //Enter the 5 first characters of the Field Value
                fieldForm.SendKeys(datum.FieldValue.Substring(0, 5));

                //Now, an ul element should have appeared. 
                new WebDriverWait(driver, TimeSpan.FromMilliseconds(5000)).Until(ExpectedConditions.ElementExists(By.LinkText(datum.FieldValue)));
                IWebElement XyzLink = driver.FindElement(By.LinkText(datum.FieldValue));
                XyzLink.Click();
            }
            return true;
        }

        private bool fillWaitText(FormDatum datum, string iframe)
        {
            Thread.Sleep(2500);
            return fillTextField(datum, iframe, true);
        }

        private bool fillTextField(FormDatum datum, string iframe, bool safeType)
        {
            try
            {
                IWebElement fieldForm = driver.FindElement(By.Id(datum.FieldId));
                new WebDriverWait(driver, TimeSpan.FromMilliseconds(500)).Until(ExpectedConditions.ElementIsVisible(By.Id(datum.FieldId)));
                if (fieldForm.Enabled && fieldForm.Displayed)
                {
                    if (safeType)
                    {
                        
                        //Click on the element to get the focus
                        fieldForm.Click();
                        fieldForm.SendKeys(Keys.Control + "a");
                        fieldForm.SendKeys(datum.FieldValue);
                    }
                    else
                    {
                        fieldForm.Clear();
                        fieldForm.SendKeys(datum.FieldValue);
                    }

                    return true;

                }
                else
                {
                    throw new Exception("Element +" + datum.FieldId + " it is not ready yet");
                }
            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
           
        }
        
        private void fillForm(List<FormDatum> data, String iframe, bool safeType)
        {
            int fieldsNotFilled = 0;
            bool nextTrySafe = false;   //If a field fails in its filling, the next one will be filled in safe mode
            if (safeType)
                nextTrySafe = true;
            foreach (FormDatum datum in data)
            {
                try
                {
                    typeTextIntoField(datum, iframe, nextTrySafe);
                    if (!safeType)
                        nextTrySafe = false;
                }
                catch (Exception ex)
                {
                    fieldsNotFilled++;
                    nextTrySafe = true;
                    if (fieldsNotFilled > 3)
                    {
                        throw new Exception("Too much fields missed at the same step");
                    }
                }
            }
        }

        private bool checkIfFormIsLoaded(List<FormDatum> data, String currently_iframe, String id_button_previous_step="", String iframe_previous_step="")
        {

            foreach (FormDatum datum in data)
            {
                try
                {
                    getIwebElement(datum.FieldId, currently_iframe, id_button_previous_step, iframe_previous_step);
                }catch(Exception ex)
                {
                    return false;
                }         
            }
                return true;
        }

        private IWebElement getIwebElement(String idElement, String currently_iframe = null, String id_button_previous_step = "", String iframe_previous_step = "")
        {
            IWebElement element = null;
            IWebElement detailFrame;
            int numRetries = 0;
            bool element_picked = false;

            while (!element_picked)
            {
                try
                {
                    new WebDriverWait(driver, TimeSpan.FromMilliseconds(500)).Until(ExpectedConditions.ElementExists(By.Id(idElement)));
                    element = driver.FindElement(By.Id(idElement));
                    element_picked = true;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(500);
                    numRetries++;
                    if (numRetries == 20)
                    {
                        //DOM and WebSelenium could be not synchronized. Let's try to click on the button of the previuous step.
                        recoverStageFromPreviousPage(id_button_previous_step, iframe_previous_step);
                    }

                    if (numRetries > 10 && currently_iframe != null && currently_iframe != "")
                    {
                        //Maybe the element is placed into another iframe, which id was suplied as parameter. Let's try to load that iframe...
                        driver.SwitchTo().DefaultContent();
                        new WebDriverWait(driver, TimeSpan.FromSeconds(15)).Until(ExpectedConditions.ElementExists((By.XPath("//iframe[@id='" + currently_iframe + "']"))));
                        detailFrame = driver.FindElement(By.XPath("//iframe[@id='" + currently_iframe + "']"));
                        driver.SwitchTo().Frame(detailFrame);
                    }

                    if (numRetries > 30)
                    {
                        //To many rertries...
                        throw new Exception(ex.Message);
                    }
                }
            }
            return element;
        }

        private void recoverStageFromPreviousPage(String id_button_previous_step, String iframe_previous_step)
        {
            if (id_button_previous_step != null && id_button_previous_step != "" && iframe_previous_step != null && iframe_previous_step != "")
            {
                int numRetries = 0;
                IWebElement detailFrame;
                bool recovered = false;
                while (!recovered)
                {
                    try
                    {
                        IWebElement button_previous = driver.FindElement(By.Id(id_button_previous_step));
                        new WebDriverWait(driver, TimeSpan.FromSeconds(1)).Until(ExpectedConditions.ElementToBeClickable(By.Id(id_button_previous_step)));
                        if (!button_previous.Selected)
                        {
                            customScrollToElementById(id_button_previous_step);
                            button_previous.Click();
                            recovered = true;
                        }
                    }
                    catch (Exception ex2)
                    {
                        numRetries++;
                        if (numRetries >= 15)
                        {
                            //Something was wrong....
                            throw new Exception("The web driver is corrupted" + ex2.Message);
                        }
                        driver.SwitchTo().DefaultContent();
                        if (iframe_previous_step != null && iframe_previous_step != "")
                        {
                            detailFrame = driver.FindElement(By.XPath("//iframe[@id='" + iframe_previous_step + "']"));
                            driver.SwitchTo().Frame(detailFrame);
                        }
                    }
                }
            }
        }

        private bool checkIfExistsInfoMessage (int maxTimeOut = 500)
        {
            return checkIfExistsMessage(Constants.Overall_Id.InfoMessageOkButtonId, Constants.Overall_Id.InfoClassBody, maxTimeOut);
        }

        private bool checkIfExistsErrorMessage(int maxTimeOut = 500, bool clickOnOK = true)
        {
            return checkIfExistsMessage(Constants.Overall_Id.ErrorMessageOkButtonId, Constants.Overall_Id.ErrorClassBody, maxTimeOut, clickOnOK);
        }
        
        private bool checkIfExistsMessage(String buttonId, String messageClass, int maxTimeOut, bool clickOnOk = true, int numWarningRetries = 25)
        {
            //Wait for an error / warning message
            try
            {
                driver.SwitchTo().DefaultContent();
                new WebDriverWait(driver, TimeSpan.FromMilliseconds(maxTimeOut)).Until(ExpectedConditions.ElementExists(By.XPath("//p[@class='"+ messageClass+"']")));
                if(clickOnOk)
                    pressButton(buttonId, "", numMaxRetry: numWarningRetries);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool checkIfExistsWarningMessage(int maxTimeOut = 500, bool clickOnOk = true, int numWarningRetries =25)
        {
            return checkIfExistsMessage(Constants.Overall_Id.WarningMessageOkButtonId, Constants.Overall_Id.WarningClassBody, maxTimeOut, clickOnOk, numWarningRetries: numWarningRetries);
        }

        private void scrollTo(int xPosition = 0, int yPosition = 0)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            var scroll = String.Format("window.scrollTo({0}, {1})", xPosition, yPosition);

            js.ExecuteScript(scroll);
        }

        private bool CheckIfStateExists(String state)
        {
            try
            {
                //Wait for IpDtail Iframe
                IWebElement element = getIwebElementByXpath("//div[text()='Tax Info']", Constants.AddCompany_Setup4.IframeData);
                //Wait for the state
                IWebElement state_element = driver.FindElement(By.XPath("//*[contains(text(),'" + state + "')]"));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool FindStateInArray(FormDatum datum)
        {
            return datum.FieldId == Constants.AddCompany_Setup4.Local_State;
        }



        /*****************************************************************************************************************************\
        |                                                      PUBLIC FUNCTIONS                                                       |
        \*****************************************************************************************************************************/

        public void searchCompany(String clientId, String companyName)
        {
            //Find the search field
            IWebElement searchField = this.getIwebElement(Constants.Overall_Id.SearchFieldText, Constants.Overall_Id.MainIFrame);
            String aux = companyName;
            //Type the company into the field
            searchField.Clear();
            searchField.SendKeys(clientId.Replace("'", ""));
            //Search the Find button and click on it
            searchField.SendKeys(Keys.Enter);
            Thread.Sleep(2000);

            //this.pressButton(Constants.Overall_Id.FindButton, Constants.Overall_Id.MainIFrame);
            //Click on the company

            if (companyName.Length == 8)
            {
                String sbs = companyName.Substring(2, 6);
                new WebDriverWait(driver, TimeSpan.FromSeconds(120)).Until(ExpectedConditions.ElementExists(By.PartialLinkText(sbs)));
                this.selectCompany(sbs);
            }
            else
            {
                new WebDriverWait(driver, TimeSpan.FromSeconds(120)).Until(ExpectedConditions.ElementExists(By.PartialLinkText(companyName)));
                this.selectCompany(companyName);
            }
        }

        public IWebElement getIwebElementByXpath(String xPath, String currently_iframe = "", String id_button_previous_step = "", String iframe_previous_step = "")
        {
            IWebElement element = null;
            IWebElement detailFrame;
            int numRetries = 0;
            bool element_picked = false;
            if (currently_iframe != null && currently_iframe != "")
            {
                driver.SwitchTo().DefaultContent();
                new WebDriverWait(driver, TimeSpan.FromSeconds(15)).Until(ExpectedConditions.ElementExists((By.XPath("//iframe[@id='" + currently_iframe + "']"))));
                detailFrame = driver.FindElement(By.XPath("//iframe[@id='" + currently_iframe + "']"));
                driver.SwitchTo().Frame(detailFrame);
            }

            while (!element_picked)
            {
                try
                {
                    new WebDriverWait(driver, TimeSpan.FromMilliseconds(500)).Until(ExpectedConditions.ElementExists((By.XPath(xPath))));
                    element = driver.FindElement(By.XPath(xPath));

                    element_picked = true;
                }
                catch (Exception ex)
                {
                    numRetries++;
                    if (numRetries == 20)
                    {
                        //DOM and WebSelenium could be not synchronized. Let's try to click on the button of the previuous step.
                        recoverStageFromPreviousPage(id_button_previous_step, iframe_previous_step);
                    }

                    if (numRetries > 10 && currently_iframe != null && currently_iframe != "")
                    {
                        //Maybe the element is placed into another iframe, which id was suplied as parameter. Let's try to load that iframe...
                        driver.SwitchTo().DefaultContent();
                        new WebDriverWait(driver, TimeSpan.FromSeconds(15)).Until(ExpectedConditions.ElementExists((By.XPath("//iframe[@id='" + currently_iframe + "']"))));
                        detailFrame = driver.FindElement(By.XPath("//iframe[@id='" + currently_iframe + "']"));
                        driver.SwitchTo().Frame(detailFrame);
                    }

                    if (numRetries > 30)
                    {
                        //To many rertries...
                        throw new Exception(ex.Message);
                    }
                }
            }
            return element;
        }

        public WebDriver(String url)
        {
            FirefoxOptions options = new FirefoxOptions();
            options.AddArguments("--headless");

            // this.driver = new FirefoxDriver(options);
            this.driver = new FirefoxDriver();
            this.url = url;
            driver.Manage().Window.Maximize();
        }

        public void Quit()
        {
            if (this.driver != null)
            {
                this.driver.Quit();
            }

        }

        public void logIn(String htmlLoginIDForm, String loginName, String htmlPasswordForm, String loginPassword)
        {
            //Perform navigation
            driver.Navigate().GoToUrl(this.url);
            //Find username field and fill it
            IWebElement loginNameElment = getIwebElement(htmlLoginIDForm);
            loginNameElment.SendKeys(loginName);

            //Find button next and click on 
            IWebElement nextButton = getIwebElement("btnNext");
            nextButton.Click();

            //Find userPassword field and fill it
            IWebElement loginPasswordElment = getIwebElement(htmlPasswordForm);
            loginPasswordElment.SendKeys(loginPassword);
            //Refresh button next and click on
            nextButton = getIwebElement("btnNext");  //TODO:HARCODED!!! 
            nextButton.Click();

            new WebDriverWait(driver, TimeSpan.FromSeconds(60)).Until(ExpectedConditions.ElementExists((By.XPath("//iframe[@id='iPDetail']"))));
            new WebDriverWait(driver, TimeSpan.FromSeconds(60)).Until(ExpectedConditions.ElementExists((By.XPath("//iframe[@id='iPDetail']"))));
            new WebDriverWait(driver, TimeSpan.FromSeconds(60)).Until(ExpectedConditions.ElementIsVisible((By.Id(Constants.Overall_Id.SearchFieldText))));

        }

        public void selectCompany(String company)
        {
            bool companySelected = false;
            int numRetries = 0;
            driver.SwitchTo().DefaultContent();
            //IWebElement detailFrame = driver.FindElement(By.XPath("//iframe[@id='" + Constants.Overall_Id.MainIFrame + "']"));
            //driver.SwitchTo().Frame(detailFrame);

            //At this point, Ajax may still filling the iframe. Robot try to select the element 
            while (!companySelected)
            {
                if (company.Length == 8)
                {
                    company = company.Remove(0, 2);
                }

                if (numRetries == 40 && company.Length == 6)
                {
                    company = "00" + company;
                }

                try
                {
                    new WebDriverWait(driver, TimeSpan.FromSeconds(15)).Until(ExpectedConditions.ElementExists(By.PartialLinkText(company)));
                    IWebElement companyTag = driver.FindElement(By.PartialLinkText(company));
                    if (companyTag.Text.Contains(company))
                    {
                        companyTag.Click();
                        companySelected = true;
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(500);
                    numRetries++;
                    if (numRetries > 60)
                    {
                        //To many rertries...
                        throw new Exception("To many retries while robot was trying to pick Company. Maybe Company does not exists");
                    }
                }
            }
        }

        public void clickOnButtonCompanyMenu(String idButton)
        {
            bool buttonSelected = false;
            int numRetries = 0;

            //Reset view of the webDriver and wait for load
            driver.SwitchTo().DefaultContent();
            try
            {
                new WebDriverWait(driver, TimeSpan.FromSeconds(45)).Until(ExpectedConditions.ElementExists((By.Id("TaskLoadingSpinner"))));
                new WebDriverWait(driver, TimeSpan.FromSeconds(45)).Until(ExpectedConditions.ElementExists((By.Id("setuppanelLoadingSpinner"))));

                new WebDriverWait(driver, TimeSpan.FromSeconds(45)).Until(ExpectedConditions.InvisibilityOfElementLocated((By.Id("TaskLoadingSpinner"))));
                new WebDriverWait(driver, TimeSpan.FromSeconds(45)).Until(ExpectedConditions.InvisibilityOfElementLocated((By.Id("setuppanelLoadingSpinner"))));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            pressButton(idButton, "");

        }

        public void recoverStageForSetupCompany()
        {
            //Find the company name and save it
            new WebDriverWait(driver, TimeSpan.FromMilliseconds(5000)).Until(ExpectedConditions.ElementIsVisible(By.ClassName("ssocompanyname-li")));
            IWebElement elementCompanyLabel = driver.FindElement(By.ClassName("ssocompanyname-li"));
            String companyName = elementCompanyLabel.Text;

            // Remove the "Company " substring(First 8 characters of the label). Pick up only the Company number, without company ID, which starts with '(' 
            companyName = companyName.Substring(8, companyName.IndexOf("("));
            companyName = companyName.Substring(0, companyName.IndexOf("(")).Trim();    //Pick up company name

            //Pick up client Id
            String clientId = getClientId();
            //Sign out of the page and relogin again
            signOut();
            this.logIn(Constants.Overall_Id.IdUserLogin, Constants.Overall_Id.UserName, Constants.Overall_Id.PwdUserLogin, Constants.Overall_Id.UserPWD);  //TODO: HARCODED!!

            //Search the company and click on it
            this.searchCompany(clientId, companyName);
        }

        public void signOut()
        {
            driver.SwitchTo().DefaultContent();
            //Click on Sign out button
            new WebDriverWait(driver, TimeSpan.FromMilliseconds(10000)).Until(ExpectedConditions.ElementToBeClickable(By.LinkText("Sign Out")));
            driver.FindElement(By.LinkText("Sign Out")).Click();
            //Relogin into the page
            new WebDriverWait(driver, TimeSpan.FromMilliseconds(10000)).Until(ExpectedConditions.ElementToBeClickable(By.Id(Constants.Overall_Id.SignInAgain)));
            driver.FindElement(By.Id(Constants.Overall_Id.SignInAgain)).Click();

            new WebDriverWait(driver, TimeSpan.FromSeconds(45)).Until(ExpectedConditions.ElementToBeClickable((By.Id(Constants.Overall_Id.IdUserLogin))));
        }

        public void selectAddCompany(int companyType)
        {
            int numRetries = 0;
            bool clickOnMenu = false;

            while (!clickOnMenu)
            {
                try
                {
                    driver.SwitchTo().DefaultContent();

                    //Wait and find the menu
                    IWebElement buttonMenu = getIwebElement(Constants.AddCompany_Select.ButtonMenuId, "");

                    //Find the element by class and make it vissible by changing its class
                    IWebElement detailFrame = driver.FindElement(By.XPath("//ul[@class='" + Constants.AddCompany_Select.DropDownMenuClas_NonVisible + "']"));

                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    var query = String.Format(@"$('ul[class=""" + Constants.AddCompany_Select.DropDownMenuClas_NonVisible + @"""]').attr('class','" + Constants.AddCompany_Select.DropDownMenuClas_Visible + "');");
                    js.ExecuteScript(query);

                    //Set link text to company with Tax filling or non tax filling
                    String linkText = (companyType == Enums.CompanyType.Tax_Filling) ? Constants.AddCompany_Select.TaxFilling_TextLink : Constants.AddCompany_Select.NonTaxFilling_TextLink;
                    //Click on the add company link
                    new WebDriverWait(driver, TimeSpan.FromMilliseconds(5000)).Until(ExpectedConditions.ElementIsVisible(By.LinkText(linkText)));
                    IWebElement link = driver.FindElement(By.LinkText(linkText));
                    link.Click();
                    //A warning message must to appear
                    if (!checkIfExistsWarningMessage(3000, false))
                    {
                        throw new Exception("There is not warning message at the first step of the process. Something went wrong");
                    }
                    IWebElement button = getIwebElementByXpath("//button[text()='OK']");
                    button.Click();
                    //Wait until page is loaded
                    new WebDriverWait(driver, TimeSpan.FromMilliseconds(240000)).Until(ExpectedConditions.ElementExists(By.Id(Constants.AddCompany_Step1.ButtonNextId)));
                    clickOnMenu = true;
                }
                catch (Exception ex)
                {
                    if (checkIfExistsWarningMessage(3000, false))
                    {
                        try
                        {
                            driver.SwitchTo().DefaultContent();
                            IWebElement button = getIwebElementByXpath("//div[@class='dlg - buttons dlg - warning']/button[@id='OkButton']");
                            button.Click();
                            //Wait until page is loaded
                            new WebDriverWait(driver, TimeSpan.FromMilliseconds(45000)).Until(ExpectedConditions.ElementExists(By.Id(Constants.AddCompany_Step1.ButtonNextId)));
                            clickOnMenu = true;
                        }
                        catch (Exception ex2)
                        {
                            clickOnMenu = false;
                        }
                        if (!clickOnMenu)
                        {
                            try
                            {
                                driver.SwitchTo().DefaultContent();
                                IWebElement button = getIwebElementByXpath("//div[@class='dlg-buttons dlg-warning']/button[@id='OkButton']");
                                button.Click();
                                //Wait until page is loaded
                                new WebDriverWait(driver, TimeSpan.FromMilliseconds(45000)).Until(ExpectedConditions.ElementExists(By.Id(Constants.AddCompany_Step1.ButtonNextId)));
                                clickOnMenu = true;
                            }
                            catch (Exception ex3)
                            {
                                clickOnMenu = false;
                            }
                        }
                    }
                    if (!clickOnMenu)
                    {
                        numRetries++;
                        Thread.Sleep(200);
                        //Check if the robot is in a next step
                        try
                        {
                            new WebDriverWait(driver, TimeSpan.FromMilliseconds(2000)).Until(ExpectedConditions.ElementExists(By.Id(Constants.AddCompany_Step1.Employee_Access)));
                            clickOnMenu = true;
                        }
                        catch (Exception)
                        {
                            clickOnMenu = false;
                        }
                        //Return back the menu to the initial state
                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        var query = String.Format(@"$('ul[class=""" + Constants.AddCompany_Select.DropDownMenuClas_Visible + @"""]').attr('class','" + Constants.AddCompany_Select.DropDownMenuClas_NonVisible + "');");
                        js.ExecuteScript(query);
                        if (numRetries == 4)
                        {
                            throw new Exception("Too many retries: " + ex.Message);
                        }
                    }
                }
            }
        }

        public String getClientId()
        {
            String clientId = "";

            new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementExists((By.XPath("//li[@class='" + Constants.AddCompany_GetClientID.CompanyName_ClientID_Label_Class + "']"))));
            IWebElement companyLabel = driver.FindElement(By.XPath("//li[@class='" + Constants.AddCompany_GetClientID.CompanyName_ClientID_Label_Class + "']"));
            //Get and parse the company info text
            clientId = companyLabel.Text.Trim();
            clientId = clientId.Substring(clientId.IndexOf("(") + 1);
            clientId = clientId.Replace(")", "");
            clientId = clientId.Trim();
            return clientId;


        }

        public bool pressButton(String id_button, String iframe_button, String id_previous_button = "", String id_previous_iframe = "", int numMaxRetry = 25)
        {
            int numRetries = 0;
            bool buttonPressed = false;
            //Load  buttons iframe
            driver.SwitchTo().DefaultContent();
            if (iframe_button != null && iframe_button != "")
            {
                new WebDriverWait(driver, TimeSpan.FromSeconds(15)).Until(ExpectedConditions.ElementExists((By.XPath("//iframe[@id='" + iframe_button + "']"))));
                IWebElement detailFrame = driver.FindElement(By.XPath("//iframe[@id='" + iframe_button + "']"));
                //Scroll to element
                scrollToView(detailFrame);
                driver.SwitchTo().Frame(detailFrame);
            }
            while (!buttonPressed)
            {
                try
                {
                    IWebElement buttonNext = getIwebElement(id_button, iframe_button);
                    scrollToView(buttonNext);
                    new WebDriverWait(driver, TimeSpan.FromMilliseconds(500)).Until(ExpectedConditions.ElementToBeClickable(By.Id(id_button)));
                    if (!buttonNext.Selected)
                    {
                        buttonNext.Click();
                    }
                    buttonPressed = true;
                }
                catch (Exception ex)
                {
                    numRetries++;
                    if (numRetries == 20)
                    {
                        recoverStageFromPreviousPage(id_previous_button, id_previous_iframe);
                    }
                    if (numRetries == numMaxRetry)
                    {
                        throw new Exception(ex.Message);
                    }

                    Thread.Sleep(200);
                    driver.SwitchTo().DefaultContent();
                    if (iframe_button != null && iframe_button != "")
                    {
                        new WebDriverWait(driver, TimeSpan.FromSeconds(15)).Until(ExpectedConditions.ElementExists((By.XPath("//iframe[@id='" + iframe_button + "']"))));
                        IWebElement detailFrame = driver.FindElement(By.XPath("//iframe[@id='" + iframe_button + "']"));
                        //Scroll to element
                        scrollToView(detailFrame);
                        driver.SwitchTo().Frame(detailFrame);
                    }
                }
            }
            return buttonPressed;
        }
    }
}
