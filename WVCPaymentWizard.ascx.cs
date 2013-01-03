/********************************
* Author: Matt Baylor (@mattbaylor)
* Purpose: Create a user portal control that allows people to donate on Arena without logging in.
* All version history is on GitHub
* Code liberally borrowed from Arena compiled sources, Den Boice, Jason Offut and Daniel Hazelbaker.
*
* WVC-Open-Giving is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* WVC-Open-Giving is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with WVC-Open-Giving.  If not, see <http://www.gnu.org/licenses/>.
********************************/

namespace ArenaWeb.UserControls.Custom.WVC
{
    using Arena.Contributions;
    using Arena.Core;
    using Arena.Core.Communications;
    using Arena.Exceptions;
    using Arena.Organization;
    using Arena.Payment;
    using Arena.Portal;
    using Arena.Portal.UI;
    using Arena.Utility;
    using ASP;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.IO;
    using System.Xml.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Web.Profile;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using ArenaWeb.UserControls.Contributions;


    public partial class WVCPaymentWizard : PortalControl
    {

        [TextSetting("Fund List", "By default all active funds with an online name will be available.  You can override this default list of funds by specifying a comma-delimited list of specific fund IDs here.", false)]
        public string FundListSetting { get { return base.Setting("FundList", "", false); } }

        [TextSetting("Confirmation Message", "Instructions and messaging on the confirmation page", false)]
        public string ConfirmationMessageSetting { get { return base.Setting("ConfirmationMessage", "", false); } }

        [TextSetting("Thank You Message", "Instructions and messaging on the thank you page", false)]
        public string ThankYouMessageSetting { get { return base.Setting("ThankYouMessage", "", false); } }

        [TextSetting("Allowed CC", "List of Allowable Credit Cards. Should be a list of the following possibilities: AmEx, Cirrus, DinersClub, Discover, JCB, Maestro, MasterCards, Solo, Switch, Visa, Defaults to : Visa,MasterCard,Discover. Please note that Capitalization matters.", false)]
        public string AllowedCCSetting { get { return base.Setting("AllowedCC", "Visa,MasterCard,Discover", false); } }

        [TextSetting("Default Fund 1", "Preselected fund for the first fund dropdown", false)]
        public string DefaultFund1Setting { get { return base.Setting("DefaultFund1", "", false); } }

        [TextSetting("Default Fund 2", "Preselected fund for the second fund dropdown", false)]
        public string DefaultFund2Setting { get { return base.Setting("DefaultFund2", "", false); } }

        [TextSetting("Default Area Code", "Area code to use if user only gives 7 digits.", false)]
        public string DefaultAreaCodeSetting { get { return base.Setting("DefaultAreaCode", "", false); } }

        [TextSetting("Give Now Text", "Give Now Button Text.", false)]
        public string GiveNowTextSetting { get { return base.Setting("GiveNowText", "Give Now", false); } }

        [TextSetting("Forgot Login URL", "URL for the forgot Login Page", false)]
        public string ForgotLoginSetting { get { return base.Setting("ForgotLoginURL", "", false); } }

        [TextSetting("Forgot Password URL", "URL for the forgot Password Page", false)]
        public string ForgotPasswordSetting { get { return base.Setting("ForgotPasswordURL", "", false); } }

        [TextSetting("Choose Login Text", "Choose Login Button Text.", false)]
        public string ChooseLoginTextSetting { get { return base.Setting("ChooseLoginText", "Use MyWVC", false); } }

        [TextSetting("Check Image URL", "URL for a check image that shows how to locate the routing number and account number from a blank check", false)]
        public string CheckImageURLSetting { get { return base.Setting("CheckImageURL", "", false); } }

        [TextSetting("Comment Caption", "By default the comment passed to the payment processor will include a list of fund IDs and amounts that the user specified.  You can optionally prompt the user for a value to include as the comment (i.e. purpose) by including a caption here.", false)]
        public string CommentCaptionSetting { get { return base.Setting("CommentCaption", "", false); } }

        [NumericSetting("Minimum Giving Amount", "The minimum contribution amount allowed by the Online Giving Wizard. The default amount is $0.01.", false)]
        public string MinimumGivingAmountSetting { get { return base.Setting("MinimumGivingAmount", "0.01", false); } }

        [NumericSetting("Maximum Giving Amount", "The maximum contribution amount allowed by the Online Giving Wizard. The default amount is $2,147,483,647.00", false)]
        public string MaximumGivingAmountSetting { get { return base.Setting("MaximumGivingAmount", "2147483647", false); } }

        [GatewayAccountSetting("CC Payment Gateway Name", "The name of the Payment Gateway to use for Credit Card Transactions", false, AccountType.Credit_Card)]
        public string CCPaymentGatewayNameSetting { get { return base.Setting("CCPaymentGatewayName", "", false); } }

        [GatewayAccountSetting("ACH Payment Gateway Name", "The name of the Payment Gateway to use for ACH Transactions", false, AccountType.ACH)]
        public string ACHPaymentGatewayNameSetting { get { return base.Setting("ACHPaymentGatewayName", "", false); } }

        [CampusSetting("New Person Campus", "The campus a new person is assigned to when added through this module.", false)]
        public int NewPersonCampusSetting { get { return Convert.ToInt32(Setting("NewPersonCampus", "-1", false)); } }

        [LookupSetting("New Person Status", "The member status given to new persons added through this module.", true, "0b4532db-3188-40f5-b188-e7e6e4448c85")]
        public int NewPersonStatusSetting { get { return Convert.ToInt32(Setting("NewPersonStatus", "", true)); } }

        [PageSetting("Login Page", "The page that the username and password should be posted against", false)]
        public string LoginPageSetting { get { return base.Setting("LoginPage", "", false); } }

        [BooleanSetting("Test Mode", "Set the module to test mode for CSS tweaking (don't ever run like this in production...)", false, false)]
        public string TestModeSetting { get { return Setting("TestMode", "false", false); } }

        [BooleanSetting("Show Portal Login", "Show the option for logging into the portal to process giving", false, false)]
        public string ShowPortalLoginSetting { get { return Setting("ShowPortalLogin", "false", false); } }
        

        private GatewayAccount ccGatewayAcct = null;
        private GatewayAccount achGatewayAcct = null;
        private RepeatingPayment _repeatingPayment;
        private PersonAddress currentAddress = null;
        private ContributionFundCollection SelectedFunds = new ContributionFundCollection();
        private String SelectedFundsSerialized = "";
        private FundCollection AvailableFunds = new FundCollection();
        private Transaction curTrans = null;

        private Person _person;
        private bool _validateCardNumber = true;

        private const string ModuleUserName = "WVCPaymentWizard";

        public string ConfirmationNumber = "";

        protected void Page_Init(object sender, EventArgs e)
        {
            //Add all necessary javascript and css elements to the head on the page. This page uses JQuery extensively.
            HtmlGenericControl oDesktopJS = new HtmlGenericControl("script");
            oDesktopJS.Attributes.Add("type", "text/javascript");
            oDesktopJS.Attributes.Add("src", "https://ajax.googleapis.com/ajax/libs/jquery/1.7/jquery.min.js");
            Page.Header.Controls.Add(oDesktopJS);

            //Handlebars is used for the confirmation screen and thank you screen
            HtmlGenericControl oDesktopJS1 = new HtmlGenericControl("script");
            oDesktopJS1.Attributes.Add("type", "text/javascript");
            oDesktopJS1.Attributes.Add("src", ResolveUrl("js/vendor/handlebars.min.js"));
            Page.Header.Controls.Add(oDesktopJS1);

            //Modernizr is used to contol the html placeholders
            HtmlGenericControl oDesktopJS2 = new HtmlGenericControl("script");
            oDesktopJS2.Attributes.Add("type", "text/javascript");
            oDesktopJS2.Attributes.Add("src", ResolveUrl("js/vendor/modernizr.min.js"));
            Page.Header.Controls.Add(oDesktopJS2);
            
            //Using Jquery validation for form validation
            HtmlGenericControl oDesktopJS5 = new HtmlGenericControl("script");
            oDesktopJS5.Attributes.Add("type", "text/javascript");
            oDesktopJS5.Attributes.Add("src", ResolveUrl("js/vendor/jquery.validate.min.js"));
            Page.Header.Controls.Add(oDesktopJS5);

            //All custom javascript is contained in this file. See comments in file.
            HtmlGenericControl oDesktopJS3 = new HtmlGenericControl("script");
            oDesktopJS3.Attributes.Add("type", "text/javascript");
            oDesktopJS3.Attributes.Add("src", ResolveUrl("js/app.js"));
            Page.Header.Controls.Add(oDesktopJS3);

            //Credit card handling javascript file. See comments in file
            HtmlGenericControl oDesktopJS4 = new HtmlGenericControl("script");
            oDesktopJS4.Attributes.Add("type", "text/javascript");
            oDesktopJS4.Attributes.Add("src", ResolveUrl("js/cc.js"));
            Page.Header.Controls.Add(oDesktopJS4);

            //Build the allowed Credit Card list so that we can track against it.
            StringBuilder aCCList = new StringBuilder();
            aCCList.Append("var allowedCC = \"");
            aCCList.Append(AllowedCCSetting);
            aCCList.Append("\";");



            HtmlGenericControl AllowedCCList = new HtmlGenericControl("script")
            {
                InnerHtml = aCCList.ToString()
            };
            AllowedCCList.Attributes.Add("type", "text/javascript");
            AllowedCCList.Attributes.Add("id", "AllowedCardList");
            Page.Header.Controls.Add(AllowedCCList);
        }

        protected void Page_Load(object Object, EventArgs e)
        {
            //Add the module's base CSS. This file may need to be modified for your specific use case
            Page.Header.Controls.Add(
                new LiteralControl("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + ResolveUrl("css/style.css") + "\" />"));

            //Throw the module settings that need to be used in the wizard into labels so that we can pick up the text in Javascript and use it later.
            this.lbConfirmationText.Text = this.ConfirmationMessageSetting;
            this.lbThankYou.Text = this.ThankYouMessageSetting;

            //Make it obvious that we are test mode (if we are)
            if (Convert.ToBoolean(TestModeSetting))
            {
                this.lbTestMode.Text = "*** TEST MODE *** TEST MODE *** TEST MODE *** TEST MODE *** TEST MODE ***";
            }

            if (!IsPostBack)
            {
                PopulateStaticControls();

                //Set some non-postback defaults as necessary from settings
                this.imgCheckImage.ImageUrl = this.CheckImageURLSetting;
                this.tbComment.Attributes["placeholder"] = this.CommentCaptionSetting;
                if (Convert.ToBoolean(this.ShowPortalLoginSetting))
                {
                    this.hfTracker.Value = "0";
                }
                else
                {
                    this.hfTracker.Value = "1";
                }
                
                this.btnChooseLogin.Value = this.ChooseLoginTextSetting;
                this.btnGiveNow.Value = this.GiveNowTextSetting;
                this.hfLoginLocation.Value = Page.ResolveUrl("default.aspx?page="+this.LoginPageSetting.ToString()).ToString();
                this.forgotLogin.NavigateUrl = this.ForgotLoginSetting;
                this.forgotPassword.NavigateUrl = this.ForgotPasswordSetting;
                    
            }
            else
            {
                //conditional processing based on the value of the hfTracker. hfTracker keeps C# and Javascript in sync throughout the wizard
                switch(hfTracker.Value)
                {
                    case "1":

                        //figure out if we have a new or returning person. See function for specific processing details.
                        int iPersonId = GetPersonIdFromInputData();

                        //check to be sure nothing went wrong
                        if (iPersonId != 0)
                        {
                            //Set the person
                            Person person = new Person(iPersonId);
                            this._person = new Person(iPersonId);

                            //Set the funds from the Settings
                            this.SelectedFunds = GetSelectedFundCollection();

                            //Load the gateways
                            this.LoadGateways();

                            //Send off for preauth and test the result
                            if (this.SubmitPreAuthorization())
                            {
                                //All good! build the confirmation screen
                                this.buildConfirmationScreen(person);
                                this.hfConfirmationID.Value = this.ConfirmationNumber.ToString();
                                this.hfPersonID.Value = this._person.PersonID.ToString();
                                
                                //Advance the tracker
                                this.hfTracker.Value = "2";
                            }
                            else 
                            { 
                                //failure case. Error messaging is handled through a different object function, so just stop.
                                return; 
                            }
                        }
                        else
                        {
                            //failure case. Error messaging is handled through a different object function, so just stop.
                            return;
                        }
                        break;
                    case "2":
                        //Reset the person from the html. Persistence is manually handled here
                        this._person = new Person(Convert.ToInt32(this.hfPersonID.Value));

                        //Load the gateways again
                        this.LoadGateways();

                        //Send off for authorization and test for a valid transaction
                        if (this.SubmitTransaction())
                        {
                            //All good! build the thank you screen
                            this.buildThankYou();

                            //Advance the Tracker
                            this.hfTracker.Value = "3";
                        }
                        else
                        {
                            //failure case. Error messaging is handled through a different object function, so just stop.
                            return; 
                        }
                        break;
                }
            }
        }

        
        
        protected void buildConfirmationScreen(Person person)
        {
            //put the right data into the page so we can show a confirmation screen. We're going to use Javascript and Handlebars for formatting, so we're just building templates and json objects
            //Add the template to the page
            HtmlGenericControl template = new HtmlGenericControl("script")
            {
                InnerHtml = "<table cellpadding=\"0\" cellspacing=\"0\" class=\"confirmationData\">{{#each info}}<tr><td class=\"label\">{{label}}</td><td class=\"data\">{{{data}}}&nbsp;</td></tr>{{/each}}</table>"
            };
            template.Attributes.Add("type", "text/x-handlebars-template");
            template.Attributes.Add("id", "datatable-template");
            Page.Header.Controls.Add(template);

            //Set up for the confirmation data
            StringBuilder confData = new StringBuilder();

            //Add the person data (JSON Object)
            confData.Append("var personData={ info: [");
            confData.Append("{label:\"Name\",data:\"" + person.FullName + "\"},");
            confData.Append("{label:\"Email Address\",data:\"" + person.Emails.FirstActive + "\"},");
            confData.Append("{label:\"Phone\",data:\"" + person.Phones.FindByType(276) + "\"},");
            confData.Append("{label:\"Address\",data:\"" + this.currentAddress.Address.StreetLine1 + "<br>" + this.currentAddress.Address.City + ", " + this.currentAddress.Address.State + "<br>" + this.currentAddress.Address.PostalCode + "\"},");
            confData.Append("{label:\"Country\",data:\"" + this.currentAddress.Address.Country + "\"}");
            confData.Append("]};");

            confData.AppendLine();

            //Add the gift data including the fund break down
            confData.Append("var giftData={ info: [");

            foreach (ContributionFund contribFund in this.SelectedFunds) {
                Fund curFund = new Fund(Convert.ToInt16(contribFund.FundId));
                confData.Append("{label:\"" + curFund.OnlineName + "\",data:\"" + String.Format("{0:C}", contribFund.Amount) + "\"},");
            }
            confData.Append("{label:\"\",data:\"\"},");
            confData.Append("{label:\"Total\",data:\"" + String.Format("{0:C}", Convert.ToDecimal(hfTotalContribution.Value)).ToString() + "\"},");
            confData.Append("{label:\"Memo\",data:\"" + tbComment.Text + "\"}");
            confData.Append("]};");

            //Add the payment data, handle bank routing or credit card
            confData.Append("var paymentData={ info: [");
            confData.Append("{label:\"Payment Data\",data:\"" + rblPaymentMethod.SelectedItem.Text + "\"},");
            confData.Append("{label:\"Account Number\",data:\"" + MaskAccountNumber(tbAccountNumber.Text) + MaskAccountNumber(tbCCNumber.Text) + "\"},");
            if (tbRoutingNumber.Text.Length > 0)
            {
                confData.Append("{label:\"Bank Name\",data:\"" + tbBankName.Text + "\"},");
                confData.Append("{label:\"Routing Number\",data:\"" + tbRoutingNumber.Text + "\"}");
            }
            if (tbCCNumber.Text.Length > 0)
            {
                confData.Append("{label:\"Expiration Date\",data:\"" + ddlExpMonth.SelectedValue + "/" + ddlExpYear.SelectedValue + "\"}");
            }
            confData.Append("]};");

            //Add the JSON object in a script tag on the page
            HtmlGenericControl templateData = new HtmlGenericControl("script")
            {
                InnerHtml = confData.ToString()
            };

            templateData.Attributes.Add("type", "text/javascript");
            Page.Header.Controls.Add(templateData);
            return;
        }

        protected void buildThankYou()
        {
            //put the right data into the page so we can show a thank you screen. Again we're going to use Javascript and Handlebars for formatting so we're just building templates and json objects
            //Add the template to the page
            HtmlGenericControl template = new HtmlGenericControl("script")
            {
                InnerHtml = "<table cellpadding=\"0\" cellspacing=\"0\" class=\"confirmationData\">{{#each info}}<tr><td class=\"label\">{{label}}</td><td class=\"data\">{{{data}}}&nbsp;</td></tr>{{/each}}</table>"
            };
            template.Attributes.Add("type", "text/x-handlebars-template");
            template.Attributes.Add("id", "datatable-template");
            Page.Header.Controls.Add(template);

            //Set up to add the transaction data
            StringBuilder confData = new StringBuilder();

            confData.Append("var transData={ info: [");
            confData.Append("{label:\"Transaction Date\",data:\"" + this.curTrans.TransactionDate + "\"},");
            confData.Append("{label:\"Transaction Amount\",data:\"" + this.curTrans.TransactionAmount + "\"},");
            confData.Append("{label:\"Transaction ID\",data:\"" + this.curTrans.TransactionId + "\"},");
            confData.Append("{label:\"Transaction Details\",data:\"" + this.curTrans.TransactionDetail + "\"},");
            confData.Append("]};");

            //Add the JSON object onto the page
            HtmlGenericControl templateData = new HtmlGenericControl("script")
            {
                InnerHtml = confData.ToString()
            };

            templateData.Attributes.Add("type", "text/javascript");
            Page.Header.Controls.Add(templateData);
            return;
        }

        public string SerializeToString(object obj, Type objType)
        {
            //Cool function for taking an object to JSON. Ultimately not used in this module. Deprecated and will be removed in a future version
            using(MemoryStream ms = new MemoryStream()) {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(objType);
                serializer.WriteObject(ms,obj);
                ms.Position = 0;
                StreamReader reader = new StreamReader(ms);
                return reader.ReadToEnd();
            }
        }

        protected void PopulateStaticControls()
        {
            //Add data to the static controls on the page
            //Contributed by Den Boice

            //Month DropDown
            for (int month = 1; month <= 12; month++)
            {
                ddlExpMonth.Items.Add(new ListItem(string.Format("{0:00}", month), month.ToString()));
            }

            //Year DropDown
            for (int year = DateTime.Now.Year; year <= DateTime.Now.Year + 7; year++)
            {
                ddlExpYear.Items.Add(new ListItem(year.ToString(), year.ToString()));
            }

            //Account Types
            ddlAccountType.Items.Add(new ListItem("Checking", "Checking"));
            ddlAccountType.Items.Add(new ListItem("Savings", "Savings"));

            //Set up to add the funds
            ddlSelectedFund1.Items.Add(new ListItem());
            ddlSelectedFund2.Items.Add(new ListItem());
            ddlSelectedFund3.Items.Add(new ListItem());

            //Build an array from the delimited string. Easier to process
            string[] selectedFunds = this.FundListSetting.Split(',').Select(sValue => sValue.Trim()).ToArray();
            //Loop the array and add each one to the dropdowns
            foreach (string item in selectedFunds)
            {
                Fund curFund = new Fund(Convert.ToInt16(item));
                
                //Add to the HTML controls
                ddlSelectedFund1.Items.Add(new ListItem(curFund.OnlineName, item));
                ddlSelectedFund2.Items.Add(new ListItem(curFund.OnlineName, item));
                ddlSelectedFund3.Items.Add(new ListItem(curFund.OnlineName, item));

                //Keep track of the available funds as we go
                this.AvailableFunds.Add(curFund);
                
                //If this is set as the default fund for Fund Selection one, then set it to be selected
                if (item == DefaultFund1Setting)
                {
                    ddlSelectedFund1.SelectedValue = item; 
                }

                //If this is set as the defauly fund for Fund Selection two, then set it to be selected
                if (item == DefaultFund2Setting)
                {
                    ddlSelectedFund2.SelectedValue = item;
                }
            }

        }

        protected int GetPersonIdFromInputData()
        {
            //Figure out the person from the data given. Editorial: This is the special, secret, magic sauce of this control.
            //Processing logic:
            //  1. Find all the people who match the given email address
            //  2. See if any of the people have a phone number and last name that matches what was given
            //      a. Did not match on first name because of nick name variants
            //  3. See if any of the people who match on email and match on phone match on zip code
            //  4. If so, select that person, if not create a new person.

            //Set up our data
            string sFirstName = tbFirstName.Text;
            string sLastName = tbLastName.Text;
            string sEmail = tbEmail.Text;
            string sAddress = tbAddress1.Text;
            string sCity = tbCity.Text;
            string sState = tbState.Text;
            string sZip = tbZip.Text;
            string sPhone = tbPhone.Text;

            //Find all the people who match on email address
            PersonCollection personCollection = new PersonCollection();
            personCollection.LoadByEmail(sEmail);

            //Mark found person to false
            int iFoundPersonId = -1;

            //Loop the loaded people based on email
            foreach (Person person in personCollection)
            {
                //Get their phone numbers
                PersonPhone phoneSearch = new PersonPhone(person.PersonID, FormatPhone(sPhone));
                
                //Test their PhoneNumber and last name
                if ((phoneSearch.PersonID == person.PersonID) && (person.LastName.ToLower() == sLastName.ToLower()))
                {
                    //If phone and last name match, load their main (41) address
                    this.currentAddress = person.Addresses.FindByType(41);
                    //Test the first 5 digits of the zip code (nobody knows their 4 digit extension)
                    if(this.currentAddress.Address.PostalCode.Substring(0,5) == sZip ) 
                    {
                        //Set the found person to the found person if all that matches
                        iFoundPersonId = person.PersonID;
                    }
                }
            }

            //test if we found someone
            if (iFoundPersonId > 0)
            {
                //person in the db
                //easy ride...
            }
            else
            {
                //Editorial: this code largely borrowed from Daniel Hazelbaker from his Family Registration module used in the CCCEV/HDC checkin tool.
                //create the family for the person
                Family family = new Family();
                family.OrganizationID = CurrentArenaContext.Organization.OrganizationID;
                family.FamilyName = sLastName + " Family";
                
                //add person to the family
                FamilyMember familyMember = new FamilyMember();
                family.FamilyMembers.Add(familyMember);

                //
                // Ensure some of the basics are set correctly.
                //
                if ((familyMember.Campus == null || familyMember.Campus.CampusId == -1) && NewPersonCampusSetting != -1)
                    familyMember.Campus = new Campus(NewPersonCampusSetting);
                if (familyMember.MemberStatus == null || familyMember.MemberStatus.LookupID == -1)
                    familyMember.MemberStatus = new Lookup(NewPersonStatusSetting);
                if (familyMember.RecordStatus == Arena.Enums.RecordStatus.Undefined)
                    familyMember.RecordStatus = Arena.Enums.RecordStatus.Pending;
                
                
                //add person to the db
                familyMember.FirstName = sFirstName;
                familyMember.FirstName = sFirstName;
                familyMember.LastName = sLastName;
                familyMember.FamilyRole = new Lookup(new Guid("e410e1a6-8715-4bfb-bf03-1cd18051f815"));
                familyMember.Gender = Arena.Enums.Gender.Unknown;
                familyMember.MaritalStatus = new Lookup(new Guid("9C000CF2-677B-4725-981E-BD555FDAFB30"));

                //add email to db and person to email
                PersonEmail personEmail = new PersonEmail();
                personEmail.Email = sEmail;
                personEmail.Active = true;
                familyMember.Emails.Add(personEmail);

                //add address to db and person to address
                PersonAddress personAddress = new PersonAddress();
                personAddress.Address.StreetLine1 = sAddress;
                personAddress.Address.City = sCity;
                personAddress.Address.State = sState;
                personAddress.Address.PostalCode = sZip;
                personAddress.AddressType = new Lookup(41);
                personAddress.Address.Standardize();
                this.currentAddress = personAddress;
                familyMember.Addresses.Add(personAddress);

                //add phone to db and person to phone
                PersonPhone personPhone = new PersonPhone();
                personPhone.PhoneType = new Lookup(new Guid("f2a0fba2-d5ab-421f-a5ab-0c67db6fd72e"));
                familyMember.Phones.Add(personPhone);
                personPhone.Number = FormatPhone(sPhone);

                //Save All
                family.Save(ModuleUserName);
                familyMember.Save(CurrentOrganization.OrganizationID, ModuleUserName, true);
                familyMember.SaveEmails(CurrentPortal.OrganizationID, ModuleUserName);
                familyMember.SaveAddresses(CurrentPortal.OrganizationID, ModuleUserName);
                familyMember.SavePhones(CurrentPortal.OrganizationID, ModuleUserName);
                familyMember.Save(CurrentUser.Identity.Name);

                iFoundPersonId = familyMember.PersonID;
            }

            return iFoundPersonId;
        }

        private void LoadGateways()
        {
            //If needed load the gateways
            if (this.CCPaymentGatewayNameSetting.Trim() != string.Empty)
            {
                try
                {
                    this.ccGatewayAcct = new GatewayAccount(int.Parse(this.CCPaymentGatewayNameSetting.Trim()));
                }
                catch
                {
                }
            }
            if (this.ACHPaymentGatewayNameSetting.Trim() != string.Empty)
            {
                try
                {
                    this.achGatewayAcct = new GatewayAccount(int.Parse(this.ACHPaymentGatewayNameSetting.Trim()));
                }
                catch
                {
                }
            }
        }

        private string FormatPhone(string inputPhone)
        {
            //The module only allows numbers for the phone number, however we'll allow 7 or 10 digit phone numbers. if they give us a 7 digit we add the specified (module settings) area code to it
            string formattedPhone = "false";
            switch (inputPhone.Length)
            {
                case 10:
                    formattedPhone =  ("(" + inputPhone.Substring(0, 3) + ") " + inputPhone.Substring(3, 3) + "-" + inputPhone.Substring(6));
                    break;
                case 7:
                    formattedPhone = ("(" + this.DefaultAreaCodeSetting + ") " + inputPhone.Substring(0, 3) + "-" + inputPhone.Substring(3));
                    break;
                default:
                    break;

            }
            return formattedPhone;
        }

        private string MaskAccountNumber(string accountNumber)
        {
            //Only show the last four of the account number (regardless whether it is bank account or credit card account)
            string text = "";
            for (int i = 0; i < accountNumber.Length - 4; i++)
            {
                text += "X";
            }
            if (accountNumber.Length - 4 > 0)
            {
                text += accountNumber.Substring(accountNumber.Length - 4, 4);
            }
            return text;
        }

        private void DisplayError(string errorMes)
        {
            //hand any error messages off to the javascript error display routines
            this.hfErrorMessage.Value = errorMes;
        }

        private void DisplayError(string header, List<string> errorMsgs)
        {
            //Overloaded Display Error routine, still hand any error messages off the the Javascript display routines
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0}\n", header);
            stringBuilder.Append("<ul>\n");
            for (int i = 0; i < errorMsgs.Count; i++)
            {
                stringBuilder.AppendFormat("<li>{0}</li>\n", errorMsgs[i]);
            }
            stringBuilder.Append("</ul>\n");
            this.DisplayError(stringBuilder.ToString());
        }

        private ContributionFundCollection GetSelectedFundCollection()
        {
            //Get the selected funds and their values. Any drop down that isn't selected for a fund defaults to the first fund on the fund settings list.
            ContributionFundCollection contributionFundCollection = new ContributionFundCollection();
            for (int i = 1; i <= 3; i++)
            {
                DropDownList curDrop = (DropDownList)FindControlRecursive(Page, "ddlSelectedFund" + i.ToString());
                TextBox curAmount = (TextBox)FindControlRecursive(Page, "tbSelectedFund" + i.ToString() + "Amount");
                if ((curDrop.SelectedValue.Length > 0) && (curAmount.Text.Length > 0))
                {
                    contributionFundCollection.Add(new ContributionFund
                    {
                        FundId = Convert.ToInt16(curDrop.SelectedValue),
                        Amount = Convert.ToDecimal(curAmount.Text)
                    });
                }
            }
           
            return contributionFundCollection;
        }

        private bool SubmitPreAuthorization()
        {
            //Submit pre-authorization.
            //This code completely borrowed from Arena Compiled code.
            if(!Convert.ToBoolean(this.TestModeSetting)){
            try
            {

                decimal totalAmount = Convert.ToDecimal(this.hfTotalContribution.Value);
                    GatewayAccount gatewayAccount;
                    if (rblPaymentMethod.SelectedValue == "CC")
                    {
                        gatewayAccount = this.ccGatewayAcct;
                    }
                    else
                    {
                        gatewayAccount = this.achGatewayAcct;
                    }
                    Processor processorClass = Processor.GetProcessorClass(gatewayAccount.PaymentProcessor);
                    if (processorClass != null && processorClass.TransactionTypeSupported(TransactionType.PreAuth))
                    {
                        bool flag;
                        if (rblPaymentMethod.SelectedValue == "CC")
                        {
                            flag = gatewayAccount.Authorize(TransactionType.PreAuth, this.tbCCNumber.Text, this.tbCCCIN.Text, this.ddlExpMonth.SelectedValue, this.ddlExpYear.SelectedValue, -1, this._person.PersonID, this._person.FirstName, this._person.FirstName, this._person.LastName, this._person.PrimaryAddress.StreetLine1, this._person.PrimaryAddress.City, this._person.PrimaryAddress.State, this._person.PrimaryAddress.PostalCode, this._person.Phones.FindByType(276).ToString(), this._person.Emails.FirstActive, totalAmount, "", DateTime.MinValue, PaymentFrequency.Unknown, 0, this._validateCardNumber);
                        }
                        else
                        {
                            flag = gatewayAccount.AuthorizeACH(TransactionType.PreAuth, this.tbAccountNumber.Text.Trim(), this.tbRoutingNumber.Text.Trim(), this.ddlAccountType.Items[0].Selected, this._person.PersonID, this._person.FirstName, this._person.FirstName, this._person.LastName, this._person.PrimaryAddress.StreetLine1, this._person.PrimaryAddress.City, this._person.PrimaryAddress.State, this._person.PrimaryAddress.PostalCode, this._person.Phones.FindByType(276).ToString(), this._person.Emails.FirstActive, totalAmount, "", DateTime.MinValue, PaymentFrequency.Unknown, 0);
                        }
                        if (!flag)
                        {
                            this.DisplayError("Authorization of your information failed for the following reason(s):", gatewayAccount.Messages);
                            return false;
                        }
                    }
                
            }
            catch (Exception inner)
            {
                throw new ArenaApplicationException("Error occurred during preauthorization", inner);
            }

        }
            return true;
        }
        
        private bool SubmitTransaction()
        {
            //Submit the transaction
            //This code was substantially borrowed from compiled Arena Code. Added testing for being in test mode.
            if (!Convert.ToBoolean(this.TestModeSetting))
            {
                try
                {
                    ContributionFundCollection selectedFundCollection = this.GetSelectedFundCollection();
                    decimal totalAmount = Convert.ToDecimal(this.hfTotalContribution.Value);
                    bool result;
                    if (selectedFundCollection.TotalFundAmount != totalAmount)
                    {
                        this.DisplayError("There appears to be a problem with the Funds you selected.  Please return to the selection screen and review your choices.");
                        result = false;
                        return result;
                    }
                    StringBuilder stringBuilder = new StringBuilder();
                    if (this.CommentCaptionSetting == string.Empty)
                    {
                        using (List<ContributionFund>.Enumerator enumerator = selectedFundCollection.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                ContributionFund current = enumerator.Current;
                                stringBuilder.AppendFormat("F{0}:{1}  ", current.FundId.ToString(), current.Amount.ToString("C2"));
                            }
                            goto IL_F3;
                        }
                    }
                    stringBuilder.Append(this.tbComment.Text);
                IL_F3:
                    GatewayAccount gatewayAccount;
                    string accountNumber;

                    if (rblPaymentMethod.SelectedValue == "CC")
                    {
                        gatewayAccount = this.ccGatewayAcct;
                        accountNumber = this.MaskAccountNumber(this.tbCCNumber.Text);
                    }
                    else
                    {
                        gatewayAccount = this.achGatewayAcct;
                        accountNumber = this.MaskAccountNumber(this.tbAccountNumber.Text);
                    }

                    Transaction transaction = null;
                    if (gatewayAccount.RequiresPaymentGateway)
                    {
                        string confirmationID = this.hfConfirmationID.ToString();
                        gatewayAccount.ProcessorClass.PaymentFrequency = PaymentFrequency.One_Time;
                        if (gatewayAccount.Authorize(confirmationID))
                        {
                            transaction = gatewayAccount.Transaction;
                            transaction.PersonId = this._person.PersonID;
                        }
                    }
                    else
                    {
                        if (rblPaymentMethod.SelectedValue == "CC")
                        {
                            if (gatewayAccount.Authorize(TransactionType.Sale, this.tbCCNumber.Text, this.tbCCCIN.Text, this.ddlExpMonth.SelectedValue, this.ddlExpYear.SelectedValue, -1, this._person.PersonID, this._person.FirstName, this._person.FirstName, this._person.LastName, this._person.PrimaryAddress.StreetLine1, this._person.PrimaryAddress.City, this._person.PrimaryAddress.State, this._person.PostalCode, this._person.Phones.FindByType(276).ToString(), this._person.Emails.FirstActive, totalAmount, stringBuilder.ToString(), DateTime.MinValue, PaymentFrequency.One_Time, 0, this._validateCardNumber))
                            {
                                transaction = gatewayAccount.Transaction;
                            }
                        }
                        else
                        {
                            if (gatewayAccount.AuthorizeACH(TransactionType.Sale, this.tbAccountNumber.Text.Trim(), this.tbRoutingNumber.Text.Trim(), ddlAccountType.Items[0].Selected, this._person.PersonID, this._person.FirstName, this._person.FirstName, this._person.LastName, this._person.PrimaryAddress.StreetLine1, this._person.PrimaryAddress.City, this._person.PrimaryAddress.State, this._person.PrimaryAddress.PostalCode, this._person.Phones.FindByType(276).ToString(), this._person.Emails.FirstActive, totalAmount, stringBuilder.ToString(), DateTime.MinValue, PaymentFrequency.One_Time, 0))
                            {
                                transaction = gatewayAccount.Transaction;
                            }
                        }
                    }
                    if (transaction != null)
                    {
                        transaction.Save(this._person.FullName);
                        if (!transaction.Success)
                        {
                            this.DisplayError("Authorization of your information failed for the following reason(s):", gatewayAccount.Messages);
                            result = false;
                            return result;
                        }
                        this.curTrans = transaction;
                        string str = "Online Giving";
                        if (base.CurrentOrganization.Settings["GivingBatchName"] != null)
                        {
                            str = base.CurrentOrganization.Settings["GivingBatchName"];
                        }
                        BatchType batchType = Batch.GetBatchType(transaction.PaymentMethod.Guid);
                        Batch batch = new Batch(base.CurrentOrganization.OrganizationID, true, str + " " + Enum.GetName(typeof(BatchType), batchType), transaction.TransactionDate, batchType, gatewayAccount.GatewayAccountId, this._person.FullName);
                        batch.VerifyAmount += transaction.TransactionAmount;
                        batch.Save(this._person.FullName);
                        Contribution contribution = new Contribution();
                        contribution.PersonId = transaction.PersonId;
                        contribution.TransactionId = transaction.TransactionId;
                        contribution.BatchId = batch.BatchId;
                        contribution.ContributionDate = transaction.TransactionDate;
                        contribution.CurrencyAmount = transaction.TransactionAmount;
                        contribution.TransactionNumber = transaction.TransactionDetail;
                        contribution.CurrencyType = transaction.PaymentMethod;
                        contribution.AccountNumber = transaction.RepeatingPayment.AccountNumber;
                        contribution.ContributionFunds = selectedFundCollection;
                        contribution.Memo = stringBuilder.ToString();
                        contribution.Save(this._person.FullName);
                        this.ConfirmationNumber = "Confirmation Number: " + contribution.TransactionNumber;
                        try
                        {
                            OnlineGivingContribution onlineGivingContribution = new OnlineGivingContribution();
                            onlineGivingContribution.Send(base.CurrentOrganization, transaction, selectedFundCollection, accountNumber, this.tbEmail.Text);
                            goto IL_B29;
                        }
                        catch (Exception)
                        {
                            goto IL_B29;
                        }
                    }
                    this.DisplayError("Authorization of your information failed for the following reason(s):", gatewayAccount.Messages);
                    result = false;
                    return result;
                IL_B29:
                    base.Session.Clear();
                }
                catch (Exception inner)
                {
                    throw new ArenaApplicationException("Error occurred during Authorization", inner);
                }
            }
            else
            {
                //Throw out test data for css formatting help.
                this.curTrans = new Transaction();
                this.curTrans.TransactionDate = Convert.ToDateTime("1/1/1900");
                this.curTrans.TransactionDetail = "TEST TRANSACTION!!! NO CHARGE WAS PROCESSED!";
                this.curTrans.TransactionAmount = Convert.ToDecimal(0);
                this.curTrans.TransactionId = 123456789;
            }
            return true;
           
        }

    }
}