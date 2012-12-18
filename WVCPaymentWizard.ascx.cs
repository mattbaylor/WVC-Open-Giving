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
        

        private GatewayAccount ccGatewayAcct = null;
        private GatewayAccount achGatewayAcct = null;
        private RepeatingPayment _repeatingPayment;
        private PersonAddress currentAddress = null;
        private ContributionFundCollection SelectedFunds = new ContributionFundCollection();
        private String SelectedFundsSerialized = "";
        private FundCollection AvailableFunds = new FundCollection();

        private Person _person;
        private bool _validateCardNumber = true;

        private const string ModuleUserName = "WVCPaymentWizard";

        public string ConfirmationNumber = "";

        protected void Page_Init(object sender, EventArgs e)
        {
            HtmlGenericControl oDesktopJS = new HtmlGenericControl("script");
            oDesktopJS.Attributes.Add("type", "text/javascript");
            oDesktopJS.Attributes.Add("src", "https://ajax.googleapis.com/ajax/libs/jquery/1.7/jquery.min.js");
            Page.Header.Controls.Add(oDesktopJS);

            HtmlGenericControl oDesktopJS1 = new HtmlGenericControl("script");
            oDesktopJS1.Attributes.Add("type", "text/javascript");
            oDesktopJS1.Attributes.Add("src", ResolveUrl("js/vendor/handlebars.min.js"));
            Page.Header.Controls.Add(oDesktopJS1);

            HtmlGenericControl oDesktopJS2 = new HtmlGenericControl("script");
            oDesktopJS2.Attributes.Add("type", "text/javascript");
            oDesktopJS2.Attributes.Add("src", ResolveUrl("js/vendor/modernizr.min.js"));
            Page.Header.Controls.Add(oDesktopJS2);

            HtmlGenericControl oDesktopJS3 = new HtmlGenericControl("script");
            oDesktopJS3.Attributes.Add("type", "text/javascript");
            oDesktopJS3.Attributes.Add("src", ResolveUrl("js/app.js"));
            Page.Header.Controls.Add(oDesktopJS3);

            HtmlGenericControl oDesktopJS4 = new HtmlGenericControl("script");
            oDesktopJS4.Attributes.Add("type", "text/javascript");
            oDesktopJS4.Attributes.Add("src", ResolveUrl("js/cc.js"));
            Page.Header.Controls.Add(oDesktopJS4);

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
            Page.Header.Controls.Add(
                new LiteralControl("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + ResolveUrl("css/style.css") + "\" />"));

            if (!IsPostBack)
            {
                PopulateStaticControls();
                this.imgCheckImage.ImageUrl = this.CheckImageURLSetting;
                this.tbComment.Attributes["placeholder"] = this.CommentCaptionSetting;
                this.hfTracker.Value = "0";
                this.btnChooseLogin.Value = this.ChooseLoginTextSetting;
                this.btnGiveNow.Value = this.GiveNowTextSetting;
                this.hfLoginLocation.Value = Page.ResolveUrl("default.aspx?page="+this.LoginPageSetting.ToString()).ToString();
                this.forgotLogin.NavigateUrl = this.ForgotLoginSetting;
                this.forgotPassword.NavigateUrl = this.ForgotPasswordSetting;
                    
            }
            else
            {
                switch(hfTracker.Value)
                {
                    case "1":
                        //assuming all data is correct
                        int iPersonId = GetPersonIdFromInputData();
                        if (iPersonId != 0)
                        {
                            Person person = new Person(iPersonId);
                            this._person = new Person(iPersonId);
                            this.SelectedFunds = GetSelectedFundCollection();
                            /*this.SelectedFundsSerialized = this.SerializeToString(this.SelectedFunds,typeof(ContributionFundCollection));
                            this.hfSerializedFunds.Value = this.SelectedFundsSerialized;*/
                            this.LoadGateways();
                            if (this.SubmitPreAuthorization())
                            {
                                this.buildConfirmationScreen(person);
                                this.hfConfirmationID.Value = this.ConfirmationNumber.ToString();
                                this.hfPersonID.Value = this._person.PersonID.ToString();
                                this.hfTracker.Value = "2";
                            }
                            else 
                            { 
                                return; 
                            }
                        }
                        else
                        {
                            return;
                        }
                        break;
                    case "2":
                        /*DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ContributionFundCollection));
                        using(MemoryStream ms = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(hfSerializedFunds.Value.Trim()))) {
                            this.SelectedFunds = (ContributionFundCollection)serializer.ReadObject(ms);
                        }*/
                        this._person = new Person(Convert.ToInt32(this.hfPersonID.Value));
                        this.LoadGateways();
                        if (this.SubmitTransaction())
                        {
                            this.buildThankYou();
                            this.hfTracker.Value = "3";
                        }
                        else
                        { 
                            return; 
                        }
                        break;
                }
            }
        }

        
        
        protected void buildConfirmationScreen(Person person)
        {
            HtmlGenericControl template = new HtmlGenericControl("script")
            {
                InnerHtml = "<table cellpadding=\"0\" cellspacing=\"0\" class=\"confirmationData\">{{#each info}}<tr><td class=\"label\">{{label}}</td><td class=\"data\">{{{data}}}&nbsp;</td></tr>{{/each}}</table>"
            };
            template.Attributes.Add("type", "text/x-handlebars-template");
            template.Attributes.Add("id", "datatable-template");
            Page.Header.Controls.Add(template);


            StringBuilder confData = new StringBuilder();

            confData.Append("var personData={ info: [");
            confData.Append("{label:\"Name\",data:\"" + person.FullName + "\"},");
            confData.Append("{label:\"Email Address\",data:\"" + person.Emails.FirstActive + "\"},");
            confData.Append("{label:\"Phone\",data:\"" + person.Phones.FindByType(276) + "\"},");
            confData.Append("{label:\"Address\",data:\"" + this.currentAddress.Address.StreetLine1 + "<br>" + this.currentAddress.Address.City + ", " + this.currentAddress.Address.State + "<br>" + this.currentAddress.Address.PostalCode + "\"},");
            confData.Append("{label:\"Country\",data:\"" + this.currentAddress.Address.Country + "\"}");
            confData.Append("]};");

            confData.AppendLine();
            confData.Append("var giftData={ info: [");

            foreach (ContributionFund contribFund in this.SelectedFunds) {
                Fund curFund = new Fund(Convert.ToInt16(contribFund.FundId));
                confData.Append("{label:\"" + curFund.OnlineName + "\",data:\"" + String.Format("{0:C}", contribFund.Amount) + "\"},");
            }
            confData.Append("{label:\"\",data:\"\"},");
            confData.Append("{label:\"Total\",data:\"" + String.Format("{0:C}", Convert.ToDecimal(hfTotalContribution.Value)).ToString() + "\"},");
            confData.Append("{label:\"Memo\",data:\"" + tbComment.Text + "\"}");
            confData.Append("]};");

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


        }

        public string SerializeToString(object obj, Type objType)
        {
            using(MemoryStream ms = new MemoryStream()) {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(objType);
                serializer.WriteObject(ms,obj);
                ms.Position = 0;
                StreamReader reader = new StreamReader(ms);
                return reader.ReadToEnd();
            }
            
            /* Old XML Serializer
             * XmlSerializer serializer = new XmlSerializer(obj.GetType());

            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);

                return writer.ToString();
            }*/
        }

        protected void PopulateStaticControls()
        {
            for (int month = 1; month <= 12; month++)
            {
                ddlExpMonth.Items.Add(new ListItem(string.Format("{0:00}", month), month.ToString()));
            }

            for (int year = DateTime.Now.Year; year <= DateTime.Now.Year + 7; year++)
            {
                ddlExpYear.Items.Add(new ListItem(year.ToString(), year.ToString()));
            }
            ddlAccountType.Items.Add(new ListItem("Checking", "Checking"));
            ddlAccountType.Items.Add(new ListItem("Savings", "Savings"));
            ddlSelectedFund1.Items.Add(new ListItem());
            ddlSelectedFund2.Items.Add(new ListItem());
            ddlSelectedFund3.Items.Add(new ListItem());

            

            string[] selectedFunds = this.FundListSetting.Split(',').Select(sValue => sValue.Trim()).ToArray();
            foreach (string item in selectedFunds)
            {
                Fund curFund = new Fund(Convert.ToInt16(item));
                ddlSelectedFund1.Items.Add(new ListItem(curFund.OnlineName,item));
                ddlSelectedFund2.Items.Add(new ListItem(curFund.OnlineName, item));
                ddlSelectedFund3.Items.Add(new ListItem(curFund.OnlineName, item));
                this.AvailableFunds.Add(curFund);
                               
                if (item == DefaultFund1Setting)
                {
                    ddlSelectedFund1.SelectedValue = item; 
                }

                if (item == DefaultFund2Setting)
                {
                    ddlSelectedFund2.SelectedValue = item;
                }
            }

        }

        protected int GetPersonIdFromInputData()
        {
            string sFirstName = tbFirstName.Text;
            string sLastName = tbLastName.Text;
            string sEmail = tbEmail.Text;
            string sAddress = tbAddress1.Text;
            string sCity = tbCity.Text;
            string sState = tbState.Text;
            string sZip = tbZip.Text;
            string sPhone = tbPhone.Text;

            PersonCollection personCollection = new PersonCollection();
            personCollection.LoadByEmail(sEmail);

            int iFoundPersonId = -1;
            foreach (Person person in personCollection)
            {
                PersonPhone phoneSearch = new PersonPhone(person.PersonID, FormatPhone(sPhone));
                
                if ((phoneSearch.PersonID == person.PersonID) && (person.LastName.ToLower() == sLastName.ToLower()))
                {
                    iFoundPersonId = person.PersonID;
                    this.currentAddress = person.Addresses.FindByType(41);
                }
            }

            if (iFoundPersonId > 0)
            {
                //person in the db
                //easy ride...
            }
            else
            {
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
            string formattedPhone = "false";
            switch (inputPhone.Length)
            {
                case 10:
                    formattedPhone =  ("(" + inputPhone.Substring(0, 3) + ") " + inputPhone.Substring(3, 3) + "-" + inputPhone.Substring(6));
                    break;
                case 7:
                    formattedPhone = ("(719) " + inputPhone.Substring(0, 3) + "-" + inputPhone.Substring(3));
                    break;
                default:
                    break;

            }
            return formattedPhone;
        }

        private string MaskAccountNumber(string accountNumber)
        {
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

        //Stub for error messaging
        private void DisplayError(string errorMes)
        {
            this.hfErrorMessage.Value = errorMes;
        }

        private void DisplayError(string header, List<string> errorMsgs)
        {
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
            return true;
        }
        
        private bool SubmitTransaction()
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
            return true;
           
        }

    }
}