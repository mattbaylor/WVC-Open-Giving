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
		showVerification = function () {
			// TODO: Figure out fund selection?
			var fields = {
				firstName: $('[id$="tbFirstName"]').val(),
				lastName: $('[id$="tbLastName"]').val(),
				email: $('[id$="tbEmail"]').val(),
				phone: $('[id$="tbPhone"]').val(),
				address1: $('[id$="tbAddress1"]').val(),
				city: $('[id$="tbCity"]').val(),
				state: $('[id$="tbState"]').val(),
				zip: $('[id$="tbZip"]').val(),
				comment: $('[id$="tbComment"]').val(),
				paymentMethod: $('[id$="rblPaymentMethod"]:checked').text(),	// TODO: Make sure this grabs the text value of the radio button
				ccNumber: maskNumber($('input[id$="tbCCNumber"]').val()),
				expDate: $('[id$="ddlExpMonth"]').val() + '/' + $('[id$="ddlExpYear"]').val(),
				cvv: $('[id$="tbCCCIN"]').val(),
				bankName: $('[id$="tbBankName"]').val(),
				accountType: $('[id$="rblAccountType"]:checked').text(),		// TODO: Make sure this grabs the text value of the radio button
				routingNumber: $('[id$="tbRoutingNumber"]').val(),
				accountNumber: maskNumber($('[id$="tbAccountNumber"]').val())
			};

			$.get('UserControls/Custom/WVC/WvcPaymentWizard/templates/verification.html', function (text) {
				// Handlebars.compile() returns a function. Call it and pass in our data...
				var html = Handlebars.compile(text)(fields);
				$('.verification').empty().append(html);
			});
		},
		initPanels = function () {
			$('.givingWizard > ul .next').live('click', function () {
				// TODO: Validate form fields before proceeding...
				$('.givingWizard > ul').animate({ left: '+=' + $(this).parent().parent().outerWidth() });
				return false;
			});

			$('.givingWizard > ul .back').live('click', function () {
				$('.givingWizard > ul').animate({ left: '-=' + $(this).parent().parent().outerWidth() });
				return false;
			});

			$('.givingWizard > ul .verify').click(function () {
				showVerification();
				return false;
			});
		};

	$(function () {
		if (!Modernizr.input.placeholder) {
			initJsPlaceholders();
		}

		initPanels();
	});
}(jQuery));
