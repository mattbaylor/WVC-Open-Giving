﻿<%@ control language="c#" inherits="ArenaWeb.UserControls.Custom.WVC.WVCPaymentWizard" CodeFile="WVCPaymentWizard.ascx.cs" CodeBehind="WVCPaymentWizard.ascx.cs"%>
<%@ Register Src="FundAmountAllocation.ascx" TagName="FundAmountAllocation" TagPrefix="uc1" %>

<div class="givingWizard">
    <asp:HiddenField runat="server" ID="hfTracker" />
    <asp:HiddenField runat="server" ID="hfErrorMessage" />
    <asp:HiddenField runat="server" ID="hfLoginLocation" />
    <ul>
        <li class="wizardStep" id="wizStep1">
            <ul>
                <li>
                    <input type="button" name="btnChooseLogin" id="btnChooseLogin" runat="server" />
                    <input type="button" name="btnGiveNow" id="btnGiveNow" runat="server" />
                </li>
            </ul>
        </li>
        <li class="wizardStep" id="wizStep2">
            <fieldset>
                <legend>Login Information</legend>
                <ul>
                    <li>
                        <asp:TextBox runat="server" ID="tbLogin" placeholder="Email Address/Login" /><asp:HyperLink runat="server" ID="forgotLogin">forgot login?</asp:HyperLink>
                    </li>
                    <li>
                        <asp:TextBox runat="server" ID="tbPassword" placeholder="Password" TextMode="Password" /><asp:HyperLink runat="server" ID="forgotPassword">forgot password?</asp:HyperLink>
                    </li>
                    <li>
                        <input type="button" name="btnLogin" id="btnLogin" value="Login" />
                    </li>
                </ul>
            </fieldset>
        </li>
        <li class="wizardStep" id="wizStep3">
            <fieldset id="personalInformation">
                <legend>Personal Information</legend>
                <div class="instructions">All fields required.</div>
                <ul>
                    <li>
                        <asp:TextBox runat="server" ID="tbFirstName" placeholder="First Name"/>
                    </li>
                    <li>
                        <asp:TextBox runat="server" ID="tbLastName" placeholder="Last Name"/>
                    </li>
                    <li>
                        <asp:TextBox runat="server" ID="tbEmail" placeholder="Email Address"/>
                    </li>
                    <li>
                        <asp:TextBox runat="server" ID="tbPhone" placeholder="Phone Number"/>
                    </li>
                    <li>
                        <asp:TextBox runat="server" ID="tbAddress1" placeholder="Address"/>
                    </li>
                    <li>
                        <asp:TextBox runat="server" ID="tbCity" placeholder="City" />
                    </li>
                    <li>
                        <asp:TextBox runat="server" ID="tbState" placeholder="State" />
                    </li>
                    <li>
                        <asp:TextBox runat="server" ID="tbZip" placeholder="Zip Code" />
                    </li>
                </ul>
            </fieldset>
            <fieldset id="giftInformation">
                <legend>Gift Information</legend>                
                <ul>
                    <li>
                        <asp:DropDownList runat="server" ID="ddlSelectedFund1" class="fundSelector" />$<asp:TextBox runat="server" ID="tbSelectedFund1Amount" class="fundAmount" placeholder="0.00" />
                    </li>
                    <li>
                        <asp:DropDownList runat="server" ID="ddlSelectedFund2" class="fundSelector" />$<asp:TextBox runat="server" ID="tbSelectedFund2Amount" class="fundAmount" placeholder="0.00" />
                    </li>
                    <li>
                        <asp:DropDownList runat="server" ID="ddlSelectedFund3" class="fundSelector" />$<asp:TextBox runat="server" ID="tbSelectedFund3Amount" class="fundAmount" placeholder="0.00" />
                    </li>
                    <li>
                        Total: <span id="contributionTotal">$<asp:TextBox runat="server" ID="tbTotalContribution" ReadOnly="true" placeholder="0.00" /><asp:HiddenField runat="server" ID="hfTotalContribution" /></span>
                    </li>
                    <li>
                        <asp:TextBox runat="server" ID="tbComment" placeholder="Comment" />
                    </li>
                </ul>
            </fieldset>
            <fieldset id="paymentInformation">
                <legend>Payment Information</legend>
                <ul>
                    <li>
                        <asp:RadioButtonList runat="server" ID="rblPaymentMethod" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Credit Card" Value="CC" ID="rblPaymentMethodCC" Selected="True" />
                            <asp:ListItem Text="Bank Account" Value="ACH" ID="rblPaymentMethodACH" />
                        </asp:RadioButtonList>
                    </li>
                    <li>
                    <ul id="CC">
                    <li>
                        <asp:TextBox runat="server" ID="tbCCNumber" placeholder="Credit Card" />
                    </li>
                    <li>
                        <asp:TextBox runat="server" ID="tbCCCIN" placeholder="Security Code" />
                    </li>
                    <li>
                        <label for="expDetails">Expiration</label>
                        <span id="expDetails">
                        <asp:DropDownList runat="server" ID="ddlExpMonth" />
                        <asp:DropDownList runat="server" ID="ddlExpYear" />
                        </span>
                    </li>
                    <li>
                        <div class="cardImage"></div>
                    </li>
                </ul>
                <ul id="Bank">
                    <li>
                        <asp:TextBox runat="server" ID="tbBankName" placeholder="Bank Name" />
                    </li>
                    <li>
                        <label for="ddlAccountType">Account Type</label><asp:DropDownList runat="server" ID="ddlAccountType" />
                    </li>
                    <li>
                        <asp:TextBox runat="server" ID="tbRoutingNumber" placeholder="Routing Number" />
                    </li>
                    <li>
                        <asp:TextBox runat="server" ID="tbAccountNumber" placeholder="Account Number" />
                    </li>
                    <li><asp:Image runat="server" ID="imgCheckImage" /></li>
                </ul>
                    </li>
                </ul>
                
            </fieldset>
            <div class="buttonsetWrapper">
                <div class="buttonset">
                    <a href="#" class="back" title="Back">Back</a>
                    <asp:Button runat="server" ID="btnNext" Text="Next" class="nextButton" CausesValidation="false" />
                </div>
            </div>
        </li>
        <li class="wizardStep" id="wizStep4">
            <fieldset id="confirmation">
                <legend>Confirmation</legend>
                <p class="confirmationText">Please Confirm the information below. If everything is correct click the "Finish Button."</p>
                <div class="leftTable" id="personalInformationConf"></div>
                <div class="centerTable" id="giftInformationConf"></div>
                <div class="rightTable" id="paymentInformationConf"></div>
            </fieldset>
            <div class="buttonsetWrapper">
                <div class="buttonset">
                    <a href="#" class="back" title="Back">Back</a>
                    <asp:Button runat="server" ID="btnSubmit" Text="Finish" />
                </div>
            </div>
        </li>
        <li class="wizardStep" id="wizStep5">
            <asp:PlaceHolder runat="server" ID="phThankYou" />
            <input type="button" id="btnClose" value="Close" />
        </li>
    </ul>
    <div class="requiredNote">
        * Required
    </div>
</div>