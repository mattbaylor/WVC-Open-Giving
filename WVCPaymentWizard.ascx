<%@ control language="c#" inherits="ArenaWeb.UserControls.Custom.WVC.WVCPaymentWizard" CodeFile="WVCPaymentWizard.ascx.cs" CodeBehind="WVCPaymentWizard.ascx.cs"%>
<%
    /********************************
     * Author: Matt Baylor (@mattbaylor)
     * Purpose: Create a user portal control that allows people to donate on Arena without logging in.
     * All version history is on GitHub
     * This file is part of WVC-Open-Giving.
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
%>

<div class="givingWizard">
    <%// the hfTracker field is used to keep the c# code and the javascript code in sync%>
    <asp:HiddenField runat="server" ID="hfTracker" />
    <%// the hfErrorMesage field is used to pass any c# error messages to javascript for presentation%>
    <asp:HiddenField runat="server" ID="hfErrorMessage" />
    <%// Store the login page for posting the login information%>
    <asp:HiddenField runat="server" ID="hfLoginLocation" />
    <%// Store the confirmationid%>
    <asp:HiddenField runat="server" ID="hfConfirmationID" />
    <%// Store the person id once we either find the person or create a new one.%>
    <asp:HiddenField runat="server" ID="hfPersonID" />
    <%// Create a container for the test mode message. This hopefully will prevent anyone using it if left in test mode in production%>
    <asp:Label runat="server" ID="lbTestMode" />
    <ul>
        <%// Wizard Step One - Choose Login or Give Now %>
        <li class="wizardStep" id="wizStep1">
            <ul>
                <li>
                    <input type="button" name="btnChooseLogin" id="btnChooseLogin" runat="server" />
                    <input type="button" name="btnGiveNow" id="btnGiveNow" runat="server" />
                </li>
            </ul>
        </li>
        <%// Wizard Step Two - If Chosen, login %>
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
        <%// Wizard Step Three - If Chosen present all the fields for a single page giving interface %>
        <li class="wizardStep" id="wizStep3">
            <ul>
                <li><fieldset id="personalInformation">
                    <legend>Personal Information</legend>
                    <div class="instructions">All fields required.</div>
                    <ul>
                        <li>
                            <asp:TextBox runat="server" ID="tbFirstName" placeholder="First Name" class="required" />
                        </li>
                        <li>
                            <asp:TextBox runat="server" ID="tbLastName" placeholder="Last Name" class="required" />
                        </li>
                        <li>
                            <asp:TextBox runat="server" ID="tbEmail" placeholder="Email Address" class="required email" />
                        </li>
                        <li>
                            <asp:TextBox runat="server" ID="tbPhone" placeholder="Phone Number" class="required phone" />
                        </li>
                        <li>
                            <asp:TextBox runat="server" ID="tbAddress1" placeholder="Address" class="required" />
                        </li>
                        <li>
                            <asp:TextBox runat="server" ID="tbCity" placeholder="City" class="required" />
                        </li>
                        <li>
                            <asp:TextBox runat="server" ID="tbState" placeholder="State" class="required" />
                        </li>
                        <li>
                            <asp:TextBox runat="server" ID="tbZip" placeholder="Zip Code" class="required zipcode" />
                        </li>
                    </ul>
                </fieldset>
                <fieldset id="giftInformation">
                    <legend>Gift Information</legend>                
                    <ul>
                        <li>
                            <asp:DropDownList runat="server" ID="ddlSelectedFund1" class="fundSelector" />$<asp:TextBox runat="server" ID="tbSelectedFund1Amount" class="fundAmount dollar" placeholder="0.00" />
                        </li>
                        <li>
                            <asp:DropDownList runat="server" ID="ddlSelectedFund2" class="fundSelector" />$<asp:TextBox runat="server" ID="tbSelectedFund2Amount" class="fundAmount dollar" placeholder="0.00" />
                        </li>
                        <li>
                            <asp:DropDownList runat="server" ID="ddlSelectedFund3" class="fundSelector" />$<asp:TextBox runat="server" ID="tbSelectedFund3Amount" class="fundAmount dollar" placeholder="0.00" />
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
                            <asp:TextBox runat="server" ID="tbCCNumber" placeholder="Credit Card" class="num" />
                        </li>
                        <li>
                            <asp:TextBox runat="server" ID="tbCCCIN" placeholder="Security Code" class="num" />
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
                            <label for="ddlAccountType">Account Type</label><asp:DropDownList runat="server" ID="ddlAccountType" class="num" />
                        </li>
                        <li>
                            <asp:TextBox runat="server" ID="tbRoutingNumber" placeholder="Routing Number" class="num" />
                        </li>
                        <li>
                            <asp:TextBox runat="server" ID="tbAccountNumber" placeholder="Account Number" class="num" />
                        </li>
                        <li><asp:Image runat="server" ID="imgCheckImage" /></li>
                    </ul>
                        </li>
                    </ul>
                
                </fieldset></li>
                <li><div class="buttonsetWrapper">
                    <div class="buttonset">
                        <input type="button" class="back" value="Back" />
                        <asp:Button runat="server" ID="btnNext" Text="Next" class="nextButton" CausesValidation="false" />
                    </div>
                </div></li>
            </ul>
        </li>
        <%// Wizard Step Four -  after running a pre-auth show the confirmation information%>
        <li class="wizardStep" id="wizStep4">
        <ul>
            <li><fieldset id="confirmation">
                <legend>Confirmation</legend>
                <p class="confirmationText"><asp:Label ID="lbConfirmationText" runat="server" /></p>
                <div class="leftTable" id="personalInformationConf"></div>
                <div class="centerTable" id="giftInformationConf"></div>
                <div class="rightTable" id="paymentInformationConf"></div>
            </fieldset></li>
            <li><div class="buttonsetWrapper">
                <div class="buttonset">
                    <input type="button" class="back" value="Back" />
                    <asp:Button runat="server" ID="btnSubmit" Text="Finish" />
                </div>
            </div></li>
            </ul>
        </li>
        <%// Wizard Step Five - After running the transaction show the transaction results and thank the user %>
        <li class="wizardStep" id="wizStep5">
            <ul>
                <li>
                    <fieldset id="transInfo">
                        <legend>Transaction Information</legend>
                        <p class="confirmationText"><asp:Label runat="server" ID="lbThankYou" /></p>
                        <div class="thankYou" id="thankYou"></div>
                    </fieldset>
                </li>
            </ul>
        </li>
    </ul>
    <div class="requiredNote">
        * Required
    </div>
</div>