<%@ control language="c#" inherits="ArenaWeb.UserControls.Custom.WVC.WVCPaymentWizard" CodeFile="WVCPaymentWizard.ascx.cs" CodeBehind="WVCPaymentWizard.ascx.cs"%>
<%@ Register Src="FundAmountAllocation.ascx" TagName="FundAmountAllocation" TagPrefix="uc1" %>

<div class="givingWizard">
    <ul>
        <li>
            <ul>
                <li>
                    <asp:TextBox runat="server" ID="tbFirstName" PlaceHolder="First Name" />
                </li>
                <li>
                    <asp:TextBox runat="server" ID="tbLastName" PlaceHolder="Last Name" />
                </li>
                <li>
                    <asp:TextBox runat="server" ID="tbEmail" PlaceHolder="Email" />
                </li>
                <li>
                    <asp:TextBox runat="server" ID="tbPhone" PlaceHolder="Phone Number" />
                </li>
                <li>
                    <asp:TextBox runat="server" ID="tbAddress1" PlaceHolder="Address" />
                </li>
                <li>
                    <asp:TextBox runat="server" ID="tbCity" PlaceHolder="City" />
                </li>
                <li>
                    <asp:TextBox runat="server" ID="tbState" PlaceHolder="State" />
                </li>
                <li>
                    <asp:TextBox runat="server" ID="tbZip" PlaceHolder="Zip" />
                </li>
            </ul>
        </li>
        <li>
            <asp:Repeater runat="server" ID="repFundList">
                <HeaderTemplate>
                    <ul>
                </HeaderTemplate>
                <ItemTemplate>
                    <li>
                        <asp:HiddenField runat="server" ID="hfFundId" />
                        <asp:TextBox runat="server" ID="tbFund" PlaceHolder="<%# Eval("Name") %>" />
                    </li>
                </ItemTemplate>
                <FooterTemplate>
                    </ul>
                </FooterTemplate>
            </asp:Repeater>            
            <ul>
                <li>
                    <asp:TextBox runat="server" ID="tbComment" PlaceHolder="Comment" />
                </li>
            </ul>
        </li>
        <li>
            <ul>
                <li>
                    <asp:RadioButtonList runat="server" ID="rblPaymentMethod" PlaceHolder="PaymentMethod">
                        <asp:ListItem Text="Credit Card" Value="CC" />
                        <asp:ListItem Text="ACH" Value="ACH" />
                    </asp:RadioButtonList>
                </li>
            </ul>
            <ul>
                <li>
                    <asp:TextBox runat="server" ID="tbCCNumber" PlaceHolder="CCNumber" />
                </li>
                <li>
                    <asp:TextBox runat="server" ID="tbCCCIN" PlaceHolder="CCCIN" />
                </li>
                <li>
                    <asp:DropDownList runat="server" ID="ddlExpMonth" PlaceHolder="ExpMonth" />
                </li>
                <li>
                    <asp:DropDownList runat="server" ID="ddlExpYear" PlaceHolder="ExpYear" />
                </li>
            </ul>
            <ul>
                <li>
                    <asp:TextBox runat="server" ID="tbBankName" PlaceHolder="BankName" />
                </li>
                <li>
                    <asp:PlaceHolder runat="server" ID="rblAccountType" PlaceHolder="AccountType" />
                </li>
                <li>
                    <asp:TextBox runat="server" ID="tbRoutingNumber" PlaceHolder="RoutingNumber" />
                </li>
                <li>
                    <asp:TextBox runat="server" ID="tbAccountNumber" PlaceHolder="AccountNumber" />
                </li>
                <li>Add check image...</li>
            </ul>
        </li>
        <li>
            <asp:PlaceHolder runat="server" ID="phVerification" />
        </li>
    </ul>
</div>