<%@ control language="c#" inherits="ArenaWeb.UserControls.Custom.WVC.WVCPaymentWizard" CodeFile="WVCPaymentWizard.ascx.cs" CodeBehind="WVCPaymentWizard.ascx.cs"%>
<%@ Register Src="FundAmountAllocation.ascx" TagName="FundAmountAllocation" TagPrefix="uc1" %>

<div class="givingWizard">
    <ul>
        <li>
            <fieldset>
                <legend>Personal Information</legend>
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
            <div class="buttonset">
                <input type="submit" class="next" value="Next" />
                <a href="#" class="back" title="Back">Back</a>
            </div>
        </li>
        <li>
            <fieldset>
                <legend>Gift Information</legend>

                <uc1:FundAmountAllocation ID="fundAmountsControl" IsRequired="true" runat="server" />

                <ul>
                    <li>
                        <asp:TextBox runat="server" ID="tbComment" placeholder="Comment" />
                    </li>
                </ul>
            </fieldset>
            <div class="buttonset">
                <input type="submit" class="next" value="Next" />
                <a href="#" class="back" title="Back">Back</a>
            </div>
        </li>
        <li>
            <fieldset>
                <legend>Payment Information</legend>
                <ul>
                    <li>
                        <asp:RadioButtonList runat="server" ID="rblPaymentMethod">
                            <asp:ListItem Text="Credit Card" Value="CC" />
                            <asp:ListItem Text="ACH" Value="ACH" />
                        </asp:RadioButtonList>
                    </li>
                </ul>
                <ul>
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
                </ul>
                <ul>
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
            </fieldset>
            <div class="buttonset">
                <input type="submit" class="next" value="Next" />
                <a href="#" class="back" title="Back">Back</a>
            </div>
        </li>
        <li>
            <asp:PlaceHolder runat="server" ID="phVerification" />
            <asp:Button runat="server" ID="btnSubmit" Text="Submit" />
        </li>
    </ul>
</div>