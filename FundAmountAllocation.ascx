<%@ control language="C#" autoeventwireup="true" inherits="ArenaWeb.UserControls.Contributions.FundAmountAllocation, Arena" %>
<script type="text/javascript">
function FundAmounts_CalculateTotal(frm) 
{
    var nmbOfPayments = 0
    var order_total = 0
    var min = parseFloat('<%=MinimumAmount.Replace(",", "")%>')
    var max = parseFloat('<%=MaximumAmount.Replace(",", "")%>')

    // Run through all the form fields
    for (var i=0; i < frm.elements.length; ++i) {
        // Get the current field
        form_field = frm.elements[i]

        // Get the field's name
        form_name = form_field.name

        // Is it a "purpose" field?
        if (form_name.indexOf("tbPurpose_") > 0) {

            // If so, extract the price
            purpose_amt = parseFloat(form_field.value)
            // Update the order total
            if (!isNaN(purpose_amt) && purpose_amt >= min && purpose_amt <= max) {
                order_total += purpose_amt
                form_field.value = FundAmounts_Pad_With_Zeros(form_field.value, 2)
            }
        }        
        // Is it the "number of payments" field?
        if (form_name.indexOf("tbNumOfPayments") > 0) {
            // Is it enabled?
            if(!form_field.disabled)
            {// If so, extract the number
            payments = parseFloat(form_field.value)
            if(!isNaN(payments))
                nmbOfPayments = parseFloat(form_field.value)
            }            
        }
    }
    if(nmbOfPayments == 0)
    {
        document.getElementById('<%=totalRow%>').style.display = 'none';
    }
    else
    {
        document.getElementById('<%=totalRow%>').style.display = '';
        frm.<%=totalID%>.value = FundAmounts_Round_Decimals(order_total * nmbOfPayments, 2)
    }
    // Display the total rounded to two decimal places
    frm.<%=paymentTotalID%>.value = FundAmounts_Round_Decimals(order_total, 2)
    frm.<%=hiddenID%>.value = FundAmounts_Round_Decimals(order_total, 2)
}

function FundAmounts_Round_Decimals(original_number, decimals) 
{
    var result1 = original_number * Math.pow(10, decimals)
    var result2 = Math.round(result1)
    var result3 = result2 / Math.pow(10, decimals)
    return FundAmounts_Pad_With_Zeros(result3, decimals)
}

function FundAmounts_Pad_With_Zeros(rounded_value, decimal_places) 
{
    var value_string = rounded_value.toString()
    var decimal_location = value_string.indexOf(".")
    if (decimal_location == -1) 
    {
        decimal_part_length = 0    
        value_string += decimal_places > 0 ? "." : ""
    }
    else 
    {
        decimal_part_length = value_string.length - decimal_location - 1
    }
    var pad_total = decimal_places - decimal_part_length
    if (pad_total > 0) 
    {
        for (var counter = 1; counter <= pad_total; counter++) 
            value_string += "0"
	}
    return value_string
}

</script>

<input type="hidden" id="ihHiddenTotal" runat="server" style="margin:0"/>

<asp:Panel ID="pnlFunds" runat="server" CssClass="givingWizardFunds" />