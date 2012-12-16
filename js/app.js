'use strict';
jQuery.noConflict();

(function ($) {
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
		maskNumber = function (str) {
			return str.substring(0, str.length - 4).replace(/./gi, 'x') + str.substring(str.length - 4);
		},
        validateFields = function () {
            var valid = true;
            $('#personalInformation input').each(function(){
                if($(this).val().length == 0) {
                    $(this).addClass('tbError');
                    valid = false;
                }
                if($(this).attr('id').indexOf('tbEmail') != -1){
                    if(!validateEmail($(this).val())){
                        $(this).addClass('tbError');
                        valid = false;
                    }
                }
            });
            if(($('input[id$="hfTotalContribution"]').val().length == 0)||$('input[id$="hfTotalContribution"]').val() == 0) {
                $('input[id$="tbTotalContribution"]').addClass('tbError');
                valid = false;
            }
            if($('input[name$="rblPaymentMethod"]:checked').val() == "CC"){
                $('#CC input').each(function(){
                    if($(this).val().length == 0) {
                        $(this).addClass('tbError');
                        valid = false;
                    }
                    if($(this).attr('id').indexOf('tbCCNumber') != -1){
                        if(!validateCreditCard($(this).val(),allowedCC)){
                            $(this).addClass('tbError');
                            $('.requiredNote').html(function(index, oldhtml){
                                return(oldhtml + "<br>We cannot accept this type of credit card. We accept the following cards: " + allowedCC);
                            });
                            valid = false;
                        }
                    }
                });
                $('#CC select').each(function(){
                    if(($(this).attr('id').indexOf('ddlExpMonth') != -1) || ($(this).attr('id').indexOf('ddlExpYear') != -1)){
                        var ExpDate = new Date($('select[id$="ddlExpYear"]').val(),$('select[id$="ddlExpMonth"]').val()-1,30);
                        var curDate = new Date();
                        if(ExpDate.getTime() < curDate.getTime()){
                            valid = false;
                            $('select[id$="ddlExpMonth"]').addClass('tbError');
                            $('select[id$="ddlExpYear"]').addClass('tbError');
                        }
                    }
                });    
            } else {
                $('#Bank input').each(function(){
                    if($(this).val().length == 0) {
                        $(this).addClass('tbError');
                        valid = false;
                    }
                });
            }
            return valid;
        },
        validateEmail = function(emailAddress) {
            var pattern = /^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$/;
            return emailAddress.match(pattern);
        },
		initPanels = function () {
            
            $('.back').click(function () {
                if($('input[id$="hfTracker"]').val() > 0){
                    $('input[id$="hfTracker"]').val($('input[id$="hfTracker"]').val()-1);
                }
                setActivePanel();
            });
			$('.nextButton').live('click', function () {
				// TODO: Validate form fields before proceeding...

				//$('.givingWizard > ul').animate({ left: '+=' + $(this).parent().parent().outerWidth() });
                if(validateFields()) {
                    return true;
                } else {
                    $('.requiredNote').show();

                    return false;
                }  
			});

            $('input[id$="tbCCNumber"]').keypress(function(){
                var type = getCreditCardType($(this).val());
                $('.cardImage').css("background-image","url(\"UserControls/Custom/WVC/WVC-Open-Giving/img/"+type+".png\")");
            });

            $('input').change(function(){
                $(this).removeClass('tbError');
                $('input[id$="tbTotalContribution"]').removeClass('tbError');
            });

            $('select').change(function(){
                $('select[id$="ddlExpMonth"]').removeClass('tbError');
                $('select[id$="ddlExpYear"]').removeClass('tbError');
            });

            $('input[id$="hfErrorMessage"]').change(function() {
                $('.requiredNote').html(function(index, oldhtml){
                    return(oldhtml + "<br>" + $('input[id$="hfErrorMessage"]').html());
                });
                $('.requiredNote').show();
            });
			$('.givingWizard > ul .back').live('click', function () {
				$('.givingWizard > ul').animate({ left: '-=' + $(this).parent().parent().outerWidth() });
				return false;
			});

			$('.givingWizard > ul .verify').click(function () {
				showVerification();
				return false;
			});

            $('input[id$=btnGiveNow]').click(function() {
                $('input[id$="hfTracker"]').val(1);
                setActivePanel();
            });

            $('input[id$=btnChooseLogin]').click(function() {
                $('#wizStep1').hide();
                $('#wizStep2').show();
            });

            $('input[id$="btnLogin"]').click(function() {
                var pseudoForm = $('<form method="post" action="' + $('input[id$="hfLoginLocation"]').val() + '"><input type="hidden" name="ctl04$ctl01$txtLoginId" value="' + $('input[id$="tbLogin"]').val() + '" /><input type="hidden" name="ctl04$ctl01$txtPassword" value="' + $('input[id$="tbPassword"]').val() + '" /></form>');
                $('body').append(pseudoForm);
                $(pseudoForm).submit();
            });

            $('#Bank').hide();
            $('#rblPaymentMethodCC').click(function() {
                $('#Bank').hide();
                $('#CC').show();
            });
            $('#rblPaymentMethodACH').click(function() {
                $('#CC').hide();
                $('#Bank').show();
            });

		},
        
        calcAmounts = function(){
            $('.fundAmount').change(function () {
                var tot = 0;
                $('.fundAmount').each(function(){
                    if($(this).val().length > 0) {
                        tot += parseFloat($(this).val());
                    }
                });
                $('input[id$="tbTotalContribution"]').val(tot.toString());
                $('input[id$="hfTotalContribution"]').val(tot.toString());
            });
        },
        
        setActivePanel = function(){
            $('.wizardStep').each(function (index, domEle) {
                $(domEle).hide();
            });

            switch ($('input[id$="hfTracker"]').val()) {
                case "2":
                    var source = $("#datatable-template").html();
		            var template = Handlebars.compile(source);
                    $('#personalInformationConf').html(template(personData));
		            $('#giftInformationConf').html(template(giftData));
		            $('#paymentInformationConf').html(template(paymentData));
                    $('#wizStep4').show();
                    break;
                case "3":
                    $('#wizStep5').show();
                    break;
                case "1":
                    $('#wizStep3').show();
                    break;
                case "0":
                default:
                    $('#wizStep1').show();
                    break;
            }
        };

	$(function () {
		if (!Modernizr.input.placeholder) {
			initJsPlaceholders();
		}

		initPanels();
        setActivePanel();
        calcAmounts();
	});
}(jQuery));
