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
        public string FundListSetting
        {
            get
            {
                return base.Setting("FundList", "", false);
            }
        }

        [TextSetting("Comment Caption", "By default the comment passed to the payment processor will include a list of fund IDs and amounts that the user specified.  You can optionally prompt the user for a value to include as the comment (i.e. purpose) by including a caption here.", false)]
        public string CommentCaptionSetting
        {
            get
            {
                return base.Setting("CommentCaption", "", false);
            }
        }

        [NumericSetting("Minimum Giving Amount", "The minimum contribution amount allowed by the Online Giving Wizard. The default amount is $0.01.", false)]
        public string MinimumGivingAmountSetting
        {
            get
            {
                return base.Setting("MinimumGivingAmount", "0.01", false);
            }
        }

        [NumericSetting("Maximum Giving Amount", "The maximum contribution amount allowed by the Online Giving Wizard. The default amount is $2,147,483,647.00", false)]
        public string MaximumGivingAmountSetting
        {
            get
            {
                return base.Setting("MaximumGivingAmount", "2147483647", false);
            }
        }

        [GatewayAccountSetting("CC Payment Gateway Name", "The name of the Payment Gateway to use for Credit Card Transactions", false, AccountType.Credit_Card)]
        public string CCPaymentGatewayNameSetting
        {
            get
            {
                return base.Setting("CCPaymentGatewayName", "", false);
            }
        }

        [GatewayAccountSetting("ACH Payment Gateway Name", "The name of the Payment Gateway to use for ACH Transactions", false, AccountType.ACH)]
        public string ACHPaymentGatewayNameSetting
        {
            get
            {
                return base.Setting("ACHPaymentGatewayName", "", false);
            }
        }

        private GatewayAccount ccGatewayAcct = null;
        private GatewayAccount achGatewayAcct = null;

        private const string ModuleUserName = "WVCPaymentWizard";

        protected void Page_Load(object Object, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateStaticControls();
                FundCollection fundCollection = new FundCollection(CurrentArenaContext.Organization.OrganizationID);
                repFundList.DataSource = fundCollection.DataTable();
                repFundList.DataBind();
            }
            else
            {
                //assuming all data is correct
                int iPersonId = GetPersonIdFromInputData();
                if (iPersonId != 0)
                {
                    Person person = new Person(iPersonId);

                    foreach (RepeaterItem repeaterItem in repFundList.Items)
                    {
                        TextBox textBox = (TextBox)(repeaterItem.FindControl("tbFund"));
                    }
                }
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
                if ((person.FirstName.ToLower() == sFirstName.ToLower()) && (person.LastName.ToLower() == sLastName.ToLower()))
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
                //add person to the db
                Person newPerson = new Person();
                newPerson.FirstName = sFirstName;
                newPerson.LastName = sLastName;
                newPerson.RecordStatus = Arena.Enums.RecordStatus.Active;
                newPerson.Save(CurrentArenaContext.Organization.OrganizationID, ModuleUserName, true);

                //add email to db and person to email
                PersonEmail personEmail = new PersonEmail();
                personEmail.Email = sEmail;
                personEmail.PersonId = newPerson.PersonID;
                personEmail.Active = true;
                newPerson.Emails.Add(personEmail);
                newPerson.SaveEmails(CurrentArenaContext.Organization.OrganizationID, ModuleUserName);

                //add address to db and person to address
                PersonAddress personAddress = new PersonAddress();
                personAddress.Address.StreetLine1 = sAddress;
                personAddress.Address.City = sCity;
                personAddress.Address.State = sState;
                personAddress.Address.PostalCode = sZip;
                personAddress.Address.Standardize();
                personAddress.PersonID = newPerson.PersonID;
                personAddress.Address.Save(ModuleUserName, false);
                personAddress.save();
                newPerson.Addresses.Add(personAddress);
                newPerson.SaveAddresses(CurrentArenaContext.Organization.OrganizationID, ModuleUserName);

                //add phone to db and person to phone
                PersonPhone personPhone = new PersonPhone(newPerson.PersonID, sPhone);
                personAddress.save();
                newPerson.Phones.Add(personPhone);
                newPerson.SavePhones(CurrentArenaContext.Organization.OrganizationID, ModuleUserName);

                //add new family to db and person to family
                Family family = new Family();
                family.FamilyName = sLastName + " Family";
                family.Save(ModuleUserName);
                family.FamilyMembers.Add(new FamilyMember(newPerson.PersonID));
                family.Save(ModuleUserName);

                iFoundPersonId = newPerson.PersonID;
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


    }
}