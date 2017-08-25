define(['jquery'], function () {

    'use strict';

    function validateEmail(email) {
        var status = 0;
        var re = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;
        if (email === '') {
            status = 1;
        }
        if (email !== '' && !re.test(email)) {
            status = 2;
        }
        return status;
    }

    function validateInput() {
        var isValid = true;
        var email = $("#mce-EMAIL");
        var emailVal = email.val();
        $(".thank-you").empty();
        var checkEmail = validateEmail(emailVal);
        if (checkEmail == 1) {
            $(".thank-you").html("<span style='color:red'>Please fill out email name field</span>");
            email.addClass("error");
            isValid = false;
        } else if (checkEmail == 2) {
            $(".thank-you").html("<span style='color:red'>Please fill out valid email address</span>");
            email.addClass("error");
            isValid = false;
        }
        return isValid;
    }

    function submitMailchimp() {
        (function () {

            $("#mce-EMAIL").change(function () {
                if (validateInput()) {
                    $(".g-recaptcha").fadeIn('fast').css('display', 'inline-block');
                }
            });

            $('#newsletter-mailchimp-form').submit(function (e) {
                e.preventDefault();
                var $this = $(this);
                var messageEle = $(".thank-you");
                messageEle.fadeIn();
                $("#mce-EMAIL").removeClass("error");

                if (validateInput()) {
                    $("#mce-EMAIL").removeClass("error");
                    if ($('#g-recaptcha-response').val()) {
                        var formURL = $(this).attr("action");
                        $.ajax({
                            type: "GET", // GET & url for json slightly different
                            url: "//" + formURL + "/subscribe/post-json?c=?",
                            data: $this.serialize(),
                            dataType: 'json',
                            contentType: "application/json; charset=utf-8",
                            error: function (err) { messageEle.html('<span style="color:red">Could not connect to the registration server.</span>'); },
                            success: function (data) {
                                if (data.result != "success") {
                                    messageEle.html('<span style="color:red">' + data['msg'] + '</span>');
                                } else {
                                    messageEle.html('Thanks for subscribe. Please check mail to confirm.').delay(2000).fadeOut('slow');
                                    grecaptcha.reset();
                                    $("#mce-EMAIL").val('');
                                }
                            }
                        });
                    } else {
                        $(".thank-you").html("<span style='color:red'>You need to solve the ReCaptcha</span>");
                    }
                }
                return false;
            });
        })();
    }

    function init(container) {
        submitMailchimp();
    }
    return {
        init: function (container) {
            init(container);
        }
    };
});
