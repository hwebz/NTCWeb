define(['jquery', 'underscore', 'base/page-scroll-animation', 'base/request-page', 'base/modules/animate'], function ($, _, pageScrollAnimation, requestPage, animate) {

    String.prototype.isEmpty = function () {
        return this.length === 0 || !this.trim();
    };

    function isValidateEmail(email) {
        var status = 0;
        var re = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;
        if (email === '') {
            return false;
        }
        if (email !== '' && !re.test(email)) {
            return false;
        }
        
        return true;
    }

    function init(container) {
        pageScrollAnimation.init(container);
        
        requestPage.registRequestByContainer(container.find('.job-details'));

        $(".pull-right.view-more").on("click", function() {
            $("html, body").animate({
                scrollTop: ($(".section-job-opening.container-zindex-one").offset().top - 65) + 'px'
            }, 600);
            return false;
        });
    }

    var isRegisterCloseDialog = false;
    var isRegisterCloseDialogDuplicatedEmail = false;
    var isSubmitInProcess = false;

    function toggleSuccessDialog(isDuplicatedEmail) {
        if (!isDuplicatedEmail) {
            var $dialogContainer = $('#careerSignupDialog');
            if ($dialogContainer.css('display') == 'none') {
                $dialogContainer.css('display', 'block');
                animate($dialogContainer, { duration: 500, opacity: 1, delay: 500 });
                if (!isRegisterCloseDialog) {
                    isRegisterCloseDialog = true;
                    $('#careerSignupCloseButton').on('click', function () {
                        toggleSuccessDialog();
                    });
                }
            }
            else {
                animate($dialogContainer, { duration: 500, opacity: 0 }, { display: 'none' });
            }
        }
        else {
            var $dialogContainer = $('#careerSignupDialogEmail');
            if ($dialogContainer.css('display') == 'none') {
                $dialogContainer.css('display', 'block');
                animate($dialogContainer, { duration: 500, opacity: 1, delay: 500 });
                if (!isRegisterCloseDialogDuplicatedEmail) {
                    isRegisterCloseDialogDuplicatedEmail = true;
                    $('#careerSignupCloseButtonEmail').on('click', function () {
                        toggleSuccessDialog(isDuplicatedEmail);
                    });
                }
            }
            else {
                animate($dialogContainer, { duration: 500, opacity: 0 }, { display: 'none' });
            }
        }
    }

    function resetCareerSignupForm() {
        var $form = $('#careerSignupForm');
        if ($form != null && $form.length > 0) {
            $form.find('#submission-name').val('');
            $form.find('#submission-email').val('');
        }
    }

    function submitFormSignup(action, name, email) {
        $.ajax({
            type: 'POST',
            async: true,
            dataType: 'json',
            data: {
                name: name,
                email: email
            },
            url: action,
            success: function (data) {
                isSubmitInProcess = false;
                if (data.code == 1) {
                    resetCareerSignupForm();
                    toggleSuccessDialog();
                }
                else if (data.code == 0) {
                    toggleSuccessDialog(true);
                }
            },
            error: function () {
                isSubmitInProcess = false;
            }
        });
    }

    function registerSubmitForm(container) {
        var $dialogContainer = $('#careerSignupDialog');
        var $dialogDuplicatedEmailContainer = $('#careerSignupDialogEmail');

        if ($dialogContainer != null && $dialogContainer.length > 0) {
            $dialogContainer.remove();
            $('body').append($dialogContainer);
        }

        if ($dialogDuplicatedEmailContainer != null && $dialogDuplicatedEmailContainer.length > 0) {
            $dialogDuplicatedEmailContainer.remove();
            $('body').append($dialogDuplicatedEmailContainer);
        }

        var $form = container.find('#careerSignupForm');
        if ($form != null && $form.length > 0) {
            var action = $form.attr('action');
            var $name = $form.find('#submission-name');
            var $email = $form.find('#submission-email');

            $form.on('submit', function () {
                if (isSubmitInProcess)
                    return false;

                var isValid = true;
                var name = $name.val();
                var email = $email.val();

                // Validate name
                if (!name || name.isEmpty()) {
                    isValid = false;
                    $name.addClass('input-error');
                }
                else {
                    $name.removeClass('input-error');
                }

                // Validate email
                if (!email || !isValidateEmail(email)) {
                    isValid = false;
                    $email.addClass('input-error');
                }
                else {
                    $email.removeClass('input-error');
                }

                if (isValid) {
                    isSubmitInProcess = true;
                    submitFormSignup(action, name, email);
                }
                
                return false;
            });
        }
    }

    return {
        init: function (container) {
            init(container);
            registerSubmitForm(container);
        }
    };
});