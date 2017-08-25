define(['jquery', 'underscore', 'lib/selectFx', 'lib/classie', 'selectize'], function ($, _, _selectFx, classie, selectize) {
    
    function validateEmail(email) {
        var status = 0;
        var re = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;
        if (email === '') {
            status = 1;
        }
        if (email!==''&&!re.test(email)) {
            status = 2;
        }
        return status;
    }
    function validatePhone(phone) {
        var re = /^[0-9-+. ]+$/;
        if (phone !== '' && !re.test(phone)) {
            return false;
        }
        return true;
    }

    function validateInput() {
        var isValid = true;
        var email = $("#Email").val();
        var phone = $("#Phone").val();
        var fullName = $("#Name").val();
        $(".error-message").empty();
        if (fullName === '') {
            $("#error-for-Name").append("Please fill out Full Name field");
            isValid = false;
        }
        var checkEmail = validateEmail(email);
        if (checkEmail == 1) {
            $("#error-for-Email").append("Please fill out Email Name field");
            isValid = false;
        }
        if (checkEmail == 2) {
            $("#error-for-Email").append("Email is invalid");
            isValid = false;
        }
        var checkPhone = validatePhone(phone);
        if (!checkPhone) {
            $("#error-for-Phone").append('Phone is invalid');
            isValid = false;
        }
        if (!isValid) {
            $("html, body").animate({
                scrollTop: ($("#Name").offset().top - 85) + 'px'
            }, 600);
        }
        return isValid;
    }
    function init(container) {
        (function () {
            //if ($('.xform-container').length > 0) {
            //    $('.xform-container').replaceWith(function () {
            //        return $('div', this);
            //    });
            //}

            $('#btnSubmit').click(function (event) {
                var isValid = validateInput();
                var loading = $("#imgLoading");
                var thankMessage = $('#thank-message');
                var submitUrl = $('#currentPage').val()+'Submit';

                var ctform = $('#ct-form');
                thankMessage.empty();
                if (isValid) {
                    loading.css('display','');
                    ctform.attr('style', 'background:black;opacity:0.5');
                    $.ajax({
                        type: "POST",
                        url: submitUrl,                    
                        data: {
                            FullName: $('#Name').val(),
                            Phone: $('#Phone').val(),
                            Email: $('#Email').val(),
                            Message: $('#message').val(),
                            Country: $('#Country option:selected').text(),
                            "g-recaptcha-response" : $('#g-recaptcha-response').val()
                         },
                        dataType: 'json',
                        success: function (data) {
                            if (data.status === 'sucess') {
                                thankMessage.append('Thank you for contacting us. We will get back to you as soon as possible');
                                $('html, body').animate({
                                    scrollTop: $(".section-our-contact-form").offset().top
                                }, 500);
                                ctform.attr('style', 'display:none');
                            }
                            else if (data.status === 'invalidRecapcha') {
                                loading.css('display', 'none');
                                $('#inValidCapcha').append('You need to solve the ReCaptcha');
                                ctform.removeAttr('style', 'background:black;opacity:0.5');
                            }
                            else {
                                loading.css('display', 'none');
                                alert("We couldn't submit the form. Please try again or email us at info@niteco.com.");
                                ctform.removeAttr('style', 'background:black;opacity:0.5');
                            }
                        },
                        error: function (error) {
                            loading.css('display', 'none');
                        }
                    });
                }
                
            });
            $('#Name').on('input', function () {
                var name = $('#Name').val();
                if (name !== '') {
                    $("#error-for-Name").empty();
                } else {
                    $("#error-for-Name").show();
                }
            });
            $('#Email').on('input', function () {
                var email = $('#Email').val();
                $("#error-for-Email").empty();
                if (email ==='') {
                    $("#error-for-Email").append("Please fill out Email Address field");
                }
            });
            var countryName = document.getElementById('countryName').value;
            var phoneCode = document.getElementById('phoneCode').value;
            var listCountry = document.getElementById('listCountry').value;
            var ddlCountry = $("#Country");
            //Bind data to Country dropdown list
            if (listCountry !== '') {
                ddlCountry.append(listCountry);
            }
            ddlCountry.append('<option value="Other">Other</option>');
            //Set phone code 
            $("#Phone").val(phoneCode);
            //Set Country by Ip Address
            if (countryName !== '') {
                $('#Country').val(countryName);
            }
            var oldPhoneCode = phoneCode;
            ddlCountry.change(function () {
                var country = $("#Country").val();
                $.ajax({
                    type: "GET",
                    url: "/contact-us/GetPhoneCodeByCountryCode?countryCode=" + country,
                    dataType: 'json',
                    success: function (data) {
                        var oldPhoneNumber = $("#Phone").val();
                        var newPhoneCode = data.phone;
                        var newPhoneNumber = oldPhoneNumber.replace(oldPhoneCode, newPhoneCode);
                        oldPhoneCode = newPhoneCode;
                        $("#Phone").val(newPhoneNumber);
                    }
                });
            });
            
            var fileref = document.createElement('script');
            fileref.setAttribute("type", "text/javascript");
            fileref.setAttribute("src", "https://www.google.com/recaptcha/api.js");

            var contactForm = document.getElementById("contact-form");

            if (contactForm) {
                contactForm.appendChild(fileref);
            }


            //[].slice.call(document.querySelectorAll('input.input__field')).forEach(function (inputEl) {
            //    // in case the input is already filled..
            //    if (inputEl.value.trim() !== '') {
            //        classie.add(inputEl.parentNode, 'input--filled');
            //    }

            //    // events:
            //    inputEl.addEventListener('focus', onInputFocus);
            //    inputEl.addEventListener('blur', onInputBlur);
            //});

            //function onInputFocus(ev) {
            //    classie.add(ev.target.parentNode, 'input--filled');
            //}

            //function onInputBlur(ev) {
            //    if (ev.target.value.trim() === '') {
            //        classie.remove(ev.target.parentNode, 'input--filled');
            //    }
            //}

            //$('#Country').selectize();
        })();
    }
    return {
        init: function (container) {
            init(container);
        }
    };
});
