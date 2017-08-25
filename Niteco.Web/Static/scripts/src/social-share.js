define(['jquery', 'underscore', 'base/page-scroll-animation', 'vendor/mobile-detect'], function ($, _, pageScrollAnimation, MobileDetect) {

    'use strict';

    var md = new MobileDetect(window.navigator.userAgent);

    function init(container) {
        pageScrollAnimation.init(container);
    }

    function initScrollDown(container) {
        // Detect if mobile then set no-translate.
        if (md.mobile()) {
            $(container).addClass("no-translate");
        }

        // For desktop, set the position fixed and set top to px if on Mac.
        if ($(window).width() >= 768) {
            $(container).css("position", "fixed");
            if (navigator.appVersion.indexOf("Mac") != -1) {
                $(container).css("top", "250px");
            }
        }
        // For mobile, we start with put it in the bottom, opacity 1
        else
        {
            //$(container).css("position", "static");
            //$(container).css("opacity", "0");
        }

        $(window).scroll(function () {
            var showPos = $(window).height() * 1.2;
            if ($(window).width() >= 768) {
                // always set position fixed for desktop
                showPos = 1200;
                $(container).css("position", "fixed");

                if (navigator.appVersion.indexOf("Mac") != -1) {
                    $(container).css("top", "250px");
                }
            }

            var yPos = $(window).scrollTop() + $(window).height();
            // hide the social bar when it scroll to first screen
            if (yPos <= showPos) {
                $(container).css("opacity", "0");
            }
            else {
                $(container).css("opacity", "1");
            }

            if ($(window).width() < 768) {
                // For safari on Mac, the window width is small, set top to auto.
                if (navigator.appVersion.indexOf("Mac") != -1) {
                    $(container).css("top", "auto");
                }

                // Set correct position depends it go to bottom or not
                if (yPos >= $(document).height() - $(".footer-page").height()) {
                    $(container).css("position", "static").find('.sticky').removeClass('fixed');
                }

                if (yPos < $(document).height() - $(".footer-page").height() - 5) {
                    $(container).css("position", "fixed").find('.sticky').addClass('fixed');
                }
            }
        });        
    }

    return {
        init: function (container) {
            init(container);
            initScrollDown(container);
        }
    };
});
