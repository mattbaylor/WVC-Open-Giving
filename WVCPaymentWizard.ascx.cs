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
    using System.Web.Profile;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using ArenaWeb.UserControls.Contributions;

    public partial class WVCPaymentWizard : PortalControl
    {

        [TextSetting("Fund List", "By default all active funds with an online name will be available.  You can override this default list of funds by specifying a comma-delimited list of specific fund IDs here.", false)]
        public string FundListSetting { get { return base.Setting("FundList", "", false); } }

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
        

        private GatewayAccount ccGatewayAcct = null;
        private GatewayAccount achGatewayAcct = null;
        private RepeatingPayment _repeatingPayment;

        private Person _person;
        private bool _validateCardNumber = true;

        private const string ModuleUserName = "WVCPaymentWizard";

        public string ConfirmationNumber;

        protected void Page_Load(object Object, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateStaticControls();
                this.fundAmountsControl.OrganizationID = base.CurrentOrganization.OrganizationID;
                this.fundAmountsControl.MinimumAmount = this.MinimumGivingAmountSetting;
                this.fundAmountsControl.MaximumAmount = this.MaximumGivingAmountSetting;
                this.fundAmountsControl.FundList = this.FundListSetting;
                this.imgCheckImage.ImageUrl = this.CheckImageURLSetting;
                this.tbComment.Attributes["placeholder"] = this.CommentCaptionSetting;
            }
            else
            {
                //assuming all data is correct
                int iPersonId = GetPersonIdFromInputData();
                if (iPersonId != 0)
                {
                    Person person = new Person(iPersonId);
                    this._person = new Person(iPersonId);
                }
                this.SubmitTransaction();

            }
        }

        protected void PopulateStaticControls()
        {
            for (int month = 0; month <= 12; month++)
            {
                ddlExpMonth.Items.Add(new ListItem(string.Format("{0:00}", month), month.ToString()));
            }

            for (int year = DateTime.Now.Year; year <= DateTime.Now.Year + 7; year++)
            {
                ddlExpYear.Items.Add(new ListItem(year.ToString(), year.ToString()));
            }
            ddlAccountType.Items.Add(new ListItem("Checking", "Checking"));
            ddlAccountType.Items.Add(new ListItem("Savings", "Savings"));
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
            foreach (FundAmount current in this.fundAmountsControl.SelectedFundAmounts)
            {
                contributionFundCollection.Add(new ContributionFund
                {
                    FundId = current.FundId,
                    Amount = current.Amount
                });
            }
            return contributionFundCollection;
        }

        private bool IsPaymentInformationValid(string sPaymentType)
        {
            if (sPaymentType == "Credit Card")
            {
                if (this.rfvCCNumber.IsValid && this.rfvCCCIN.IsValid && this.rfvExpMonth.IsValid && this.rfvExpYear.IsValid)
                {
                    return true;
                }
            }
            else
            {
                if (this.rfvBankName.IsValid && this.rfvRoutingNumber.IsValid && this.rfvAccountNumber.IsValid)
                {
                    return true;
                }
            }
            return false;
        }

        private bool SubmitTransaction()
        {
            try
            {
                bool flag = this._repeatingPayment != null && this._repeatingPayment.RepeatingPaymentId != -1;
                ContributionFundCollection selectedFundCollection = this.GetSelectedFundCollection();
                decimal totalAmount = this.fundAmountsControl.TotalAmount;
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
                if (flag)
                {
                    gatewayAccount = this._repeatingPayment.GatewayAccount;
                    accountNumber = this._repeatingPayment.AccountNumber;
                }
                else
                {
                    if (this.rblPaymentMethod.SelectedValue == "Credit Card")
                    {
                        gatewayAccount = this.ccGatewayAcct;
                        accountNumber = this.MaskAccountNumber(this.tbCCNumber.Text);
                    }
                    else
                    {
                        gatewayAccount = this.achGatewayAcct;
                        accountNumber = this.MaskAccountNumber(this.tbAccountNumber.Text);
                    }
                }
                int numberOfPayments = 0;

                Transaction transaction = null;
                if (gatewayAccount.RequiresPaymentGateway)
                {
                    string confirmationID = base.Request.QueryString["confid"];
                    gatewayAccount.ProcessorClass.PaymentFrequency = PaymentFrequency.One_Time;
                    if (gatewayAccount.Authorize(confirmationID))
                    {
                        transaction = gatewayAccount.Transaction;
                        transaction.PersonId = this._person.PersonID;
                    }
                }
                else
                {
                    if (this.rblPaymentMethod.SelectedValue == "Credit Card")
                    {
                        if (gatewayAccount.Authorize(TransactionType.Sale, this.tbCCNumber.Text, this.tbCCCIN.Text, this.ddlExpMonth.SelectedValue, this.ddlExpYear.SelectedValue, -1, this._person.PersonID, this.tbFirstName.Text.Trim(), this.tbFirstName.Text.Trim(), this.tbLastName.Text.Trim(), this.tbAddress1.Text, this.tbCity.Text, this.tbState.Text, this.tbZip.Text.Trim(), this.tbPhone.Text.Trim(), this.tbEmail.Text.Trim(), totalAmount, stringBuilder.ToString(), DateTime.MinValue, PaymentFrequency.One_Time, 0, this._validateCardNumber))
                        {
                            transaction = gatewayAccount.Transaction;
                        }
                    }
                    else
                    {
                        if (gatewayAccount.AuthorizeACH(TransactionType.Sale, this.tbAccountNumber.Text.Trim(), this.tbRoutingNumber.Text.Trim(), this.rblAccountType.Items[0].Selected, this._person.PersonID, this.tbFirstName.Text.Trim(), this.tbFirstName.Text.Trim(), this.tbLastName.Text.Trim(), this.tbAddress1.Text.Trim(), this.tbCity.Text.Trim(), this.tbState.Text.Trim(), this.tbZip.Text.Trim(), this.tbPhone.Text.Trim(), this.tbEmail.Text.Trim(), totalAmount, stringBuilder.ToString(), DateTime.MinValue, PaymentFrequency.One_Time, 0))
                        {
                            transaction = gatewayAccount.Transaction;
                        }
                    }
                }
                if (transaction != null)
                {
                    transaction.Save(base.CurrentUser.Identity.Name);
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
                    Batch batch = new Batch(base.CurrentOrganization.OrganizationID, true, str + " " + Enum.GetName(typeof(BatchType), batchType), transaction.TransactionDate, batchType, gatewayAccount.GatewayAccountId, base.CurrentUser.Identity.Name);
                    batch.VerifyAmount += transaction.TransactionAmount;
                    batch.Save(base.CurrentUser.Identity.Name);
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
                    contribution.Save(base.CurrentUser.Identity.Name);
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