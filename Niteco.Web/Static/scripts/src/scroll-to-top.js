define(['jquery'], function ($) {

    'use strict';

    function init(container) {
        var offset = 500, // At what pixels show Back to Top Button
          scrollDuration = 700; // Duration of scrolling to top
        $(window).scroll(function () {
            if ($(this).scrollTop() > offset) {
                $('.scroll-top').fadeIn(500); // Time(in Milliseconds) of appearing of the Button when scrolling down.
            } else {
                $('.scroll-top').fadeOut(500); // Time(in Milliseconds) of disappearing of Button when scrolling up.
            }
        });
        container.click(function (event) {
            event.preventDefault();
            $('html, body').animate({
                scrollTop: 0
            }, scrollDuration);
        })
    }

    return {
        init: init
    };
});


