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
'use strict';
jQuery.noConflict();

(function ($) {
    //Set up for most of the handling on the page. Jason Offut wrote the skeleton of this and I played like Frankenstein.
	var initJsPlaceholders = function () {
			$('input[placeholder]').each(function () {
				var $this = $(this);
				$this.val($this.attr('placeholder'));
			})
				.blur(function () {
					var $this = $(this);

					if ($this.attr('placeholder') !== '' && ($this.val() === $this.attr('placeholder') || $this.val() === '')) {
						$this.val($this.attr('placeholder'));
					}
				})
				.focus(function () {
					var $this = $(this);

					if ($this.val() === $this.attr('placeholder')) {
						$this.val('');
					}
				});
		},

        //Mask any numbers that need to only show the last 4 digits
		maskNumber = function (str) {
			return str.substring(0, str.length - 4).replace(/./gi, 'x') + str.substring(str.length - 4);
		},

        //Set up all the validation for the form
        validateFields = function () {
            
            //set up for optimistic processing
            var valid = true;

            //Validate each of the personal information fields and make sure they follow the rules
            $('#personalInformation input').each(function(){
                if(!$('#frmMain').validate({showErrors:function(errorMap, errorList){return false;}}).element($(this))) {
                    
                    //If not format them for the error case
                    $(this).addClass('tbError');
                    valid = false;
                }
            });

            //Make sure that there is some value in one of the fund amout fields by testing the addition field.
            if(($('input[id$="hfTotalContribution"]').val().length == 0)||$('input[id$="hfTotalContribution"]').val() == 0) {
                
                //if not format them for the error case
                $('input[id$="tbTotalContribution"]').addClass('tbError');
                valid = false;
            }

            //test for Bank or CC
            if($('input[name$="rblPaymentMethod"]:checked').val() == "CC"){
                
                //CC
                //Make sure there is something in each of the fields
                $('#CC input').each(function(){
                    if($(this).val().length == 0) {
                        
                        //if not format for error case
                        $(this).addClass('tbError');
                        valid = false;
                    }

                    //if it's the Credit Card Number Field we'll run some additional tests
                    if($(this).attr('id').indexOf('tbCCNumber') != -1){
                        
                        // Test if card type is allowed by the module settings
                        if(!validateCreditCard($(this).val(),allowedCC)){

                            //If not, you know the drill
                            $(this).addClass('tbError');

                            //Also add a helpful note about what's allowed and what's not
                            $('.requiredNote').html(function(index, oldhtml){
                                return(oldhtml + "<br>We cannot accept this type of credit card. We accept the following cards: " + allowedCC);
                            });
                            valid = false;
                        }
                    }
                });
                
                // Now test the expiration date to make sure it's greater than today
                $('#CC select').each(function(){
                    if(($(this).attr('id').indexOf('ddlExpMonth') != -1) || ($(this).attr('id').indexOf('ddlExpYear') != -1)){
                        var ExpDate = new Date($('select[id$="ddlExpYear"]').val(),$('select[id$="ddlExpMonth"]').val()-1,30);
                        var curDate = new Date();
                        if(ExpDate.getTime() < curDate.getTime()){

                            //If not, yup format for error
                            valid = false;
                            $('select[id$="ddlExpMonth"]').addClass('tbError');
                            $('select[id$="ddlExpYear"]').addClass('tbError');
                        }
                    }
                });    
            } else {
                
                //Bank
                //This is easier, just check something is in each field
                $('#Bank input').each(function(){
                    if($(this).val().length == 0) {
                        
                        //If not, format for error
                        $(this).addClass('tbError');
                        valid = false;
                    }
                });
            }
            return valid;
        },
        //Set up the panels of the wizard
        initPanels = function () {
            
            //If we want to go back
            $('.back').click(function () {
                //Make sure we can go back
                if($('input[id$="hfTracker"]').val() > 0){
                    //Decrement the hfTracker and...
                    $('input[id$="hfTracker"]').val($('input[id$="hfTracker"]').val()-1);
                }
                //Reload
                setActivePanel();
            });

            //If we want to move forward in the wizard
			$('.nextButton').live('click', function () {
                //validate
                if(validateFields()) {
                    //Move on
                    return true;
                } else {

                    //Show any errors and stop
                    $('.requiredNote').show();
                    return false;
                }  
			});

            //Nifty little routine to output the graphic representation of the credit card number entered as they enter it...
            $('input[id$="tbCCNumber"]').keypress(function(){
                var type = getCreditCardType($(this).val());
                $('.cardImage').css("background","transparent url(\"UserControls/Custom/WVC/WVC-Open-Giving/img/"+type+".png\")");
            });

            //Prevent anything but numbers and decimal points
            $('.dollar').keydown(function(event) {
                
                //Can't use the shift key
                if(event.shiftKey){
                    event.preventDefault();
                    return false;
                }

                //Test the keyboard event that was triggered
                switch(event.which){

                    //Allow the following keys 0-9 (above the letters), 0-9 (tenkey), the '.' in the keyboard and the '.' in the tenkey
                    case 48: case 49: case 50: case 51: case 52: case 53: case 54: case 55: case 56: case 57:
                    case 8: case 9: case 96: case 37: case 39: case 40: case 97: case 98: case 99: case 100: case 101: case 102: case 103: case 104: case 105: case 110: case 190:
                        
                        //Allowed, as per normal
                        break;
                    default:
                        
                        //Not Allowed, stop and drop
                        event.preventDefault();
                        return false;
                }
            });

            //Prevent anything by numbers
            $('.phone,.zipcode,.num').keydown(function(event) {
                
                //Can't use the shift key
                if(event.shiftKey){
                    event.preventDefault();
                    return false;
                }

                //Test the incoming keyboard events
                switch(event.which){
                    //Allow all number keys, both keyboard and tenkey
                    case 48: case 49: case 50: case 51: case 52: case 53: case 54: case 55: case 56: case 57:
                    case 8: case 9: case 96: case 37: case 39: case 40: case 97: case 98: case 99: case 100: case 101: case 102: case 103: case 104: case 105:
                        
                        //Allowed, proceed
                        break;
                    default:
                        
                        //Not Allowed, stop and drop
                        event.preventDefault();
                        return false;
                }
            });

            //When any input is changed remove the error formatting
            $('input').change(function(){
                $(this).removeClass('tbError');
                $('input[id$="tbTotalContribution"]').removeClass('tbError');
            });

            //When the date fields and changed remove the error formatting
            $('select').change(function(){
                $('select[id$="ddlExpMonth"]').removeClass('tbError');
                $('select[id$="ddlExpYear"]').removeClass('tbError');
            });

            //If ever there is something in the Error message, show it.
            $('input[id$="hfErrorMessage"]').change(function() {
                $('.requiredNote').html(function(index, oldhtml){
                    return(oldhtml + "<br>" + $('input[id$="hfErrorMessage"]').html());
                });
                $('.requiredNote').show();
            });
			
            //I don't think this is used, but it would handle the back button...
            $('.givingWizard > ul .back').live('click', function () {
				$('.givingWizard > ul').animate({ left: '-=' + $(this).parent().parent().outerWidth() });
				return false;
			});

            //In step one when the give now button is clicked move to wizard step 3
            $('input[id$=btnGiveNow]').click(function() {
                $('input[id$="hfTracker"]').val(1);
                setActivePanel();
            });

            //In step one when the login button is clicked move to wizard step 2
            $('input[id$=btnChooseLogin]').click(function() {
                $('#wizStep1').hide();
                $('#wizStep2').show();
            });

            //This currently doesn't work as designed because I can't get Arena to accept the post. This basically sets up a pseudo form and then submits it for logging into Arean.
            //TODO: Fix Login
            $('input[id$="btnLogin"]').click(function() {
                var pseudoForm = $('<form method="post" action="' + $('input[id$="hfLoginLocation"]').val() + '"><input type="hidden" name="ctl04$ctl01$txtLoginId" value="' + $('input[id$="tbLogin"]').val() + '" /><input type="hidden" name="ctl04$ctl01$txtPassword" value="' + $('input[id$="tbPassword"]').val() + '" /></form>');
                $('body').append(pseudoForm);
                $(pseudoForm).submit();
            });

            //By default hide the bank functionality
            $('#Bank').hide();

            //If Credit Card is clicked then show the CC and hide the Bank
            $('#rblPaymentMethodCC').click(function() {
                $('#Bank').hide();
                $('#CC').show();
            });

            //If Bank is clicked then show the Bank and hide the CC
            $('#rblPaymentMethodACH').click(function() {
                $('#CC').hide();
                $('#Bank').show();
            });

		},
        
        //Calculate the total of the funds donated in each fund
        calcAmounts = function(){
            
            //When any fund amount changes
            $('.fundAmount').change(function () {
                
                //recalculate, set tot to 0
                var tot = 0;

                //For each fund amount
                $('.fundAmount').each(function(){
                    
                    //If there is something in the field
                    if($(this).val().length > 0) {

                        //Convert it to a float and add it to the total
                        tot += parseFloat($(this).val());
                    }
                });

                //Stick the output in both the displayed input
                $('input[id$="tbTotalContribution"]').val(tot.toString());

                //And stick it in the hidden field
                $('input[id$="hfTotalContribution"]').val(tot.toString());
            });
        },
        
        //Create the wizard step tracker function
        setActivePanel = function(){
            
            //Start by hiding everything
            $('.wizardStep').each(function (index, domEle) {
                $(domEle).hide();
            });

            //Test the value of the hidden tracker field
            switch ($('input[id$="hfTracker"]').val()) {
                
                //These are out of order...Case 0 needs to be at the end to follow the default processing
                //Confirmation step
                case "2":

                    //define the handlebars template
                    var source = $("#datatable-template").html();

                    //make it a handlebars template
		            var template = Handlebars.compile(source);

                    //fill in the data
                    $('#personalInformationConf').html(template(personData));
		            $('#giftInformationConf').html(template(giftData));
		            $('#paymentInformationConf').html(template(paymentData));

                    //show wizard step 4
                    $('#wizStep4').show();
                    break;

                //Thank you step
                case "3":
                    
                    //define the handlebars template
                    var source = $("#datatable-template").html();

                    //make it a handlebars template
		            var template = Handlebars.compile(source);

                    //show wizard step 5
                    $('#wizStep5').show();

                    //fill in the data
                    $('#thankYou').html(template(transData));
                    break;

                //Main form step
                case "1":
                    //Show wizard step three
                    $('#wizStep3').show();
                    break;

                //By default show step 1 (the choice between login or not)
                case "0":
                default:
                    $('#wizStep1').show();
                    break;
            }
        };

	//Main start function (document ready)
    $(function () {
        
        //use modernizr to control the placeholders
		if (!Modernizr.input.placeholder) {
			initJsPlaceholders();
		}

        //Set up the panels
		initPanels();

        //Show the active panel
        setActivePanel();

        //Calculate the current total
        calcAmounts();
	});
}(jQuery));
