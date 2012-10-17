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
		initVerification = function () {
			$.get('UserControls/Custom/WVC/WvcPaymentWizard/templates/verification.html', function (text) {
				// TODO: Double check selectors...
				var fields = {
						firstName: $('input[id$="firstName"]').val(),
						lastName: $('input[id$="lastName"]').val()
					},
					template = Handlbars.compile(text),
					html = template(fields);

				// TODO: Fix selector...
				$('.givingWizard > ul > li:last-child').before(html);
			});
		},
		initPanels = function () {
			// TODO: Fix selectors & implement handlers to animate panes left/right in the viewport
			$('#ball-o-wax .next').live('click', function () {
				return false;
			});

			$('#ball-o-wax .back').live('click', function () {
				return false;
			});

			$('.verify').click(function () {
				initVerification();
				return false;
			});
		};

	$(function {
		if (!Modernizr.input.placeholder) {
			initJsPlaceholders();
		}

		initPanels();
	});
}(jQuery));
