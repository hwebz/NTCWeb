define(['jquery', 'lib/jquery.prettyPhoto', 'vendor/mobile-detect'], function ($, prettyPhoto, MobileDetect) {

    'use strict';
    var md = new MobileDetect(window.navigator.userAgent);
    function init(container) {

        $("#product-features-img").hover(function () { $(this).find('img').attr("src", "/img/product-features-hover.png"); },
            function () { $(this).find('img').attr("src", "/img/product-features.png"); });
        var windowWidth = $(window).width();
        var windowHeight = $(window).height();
        var widthReview = windowWidth * 0.8;
        var heightReview = widthReview * 0.6;
        
        //$("a[rel^='prettyPhoto']").prettyPhoto(
        //        {
        //            social_tools: false,
        //            deeplinking: false,
        //            animation_speed: 'normal', theme: 'pp_default', slideshow: 3000, autoplay_slideshow: false,
        //            allow_resize: true, /* Resize the photos bigger than viewport. true/false */
        //            default_width: widthReview,
        //            default_height: heightReview,
        //        });
        if (!md.mobile()) {
            $("a[rel^='prettyPhoto']").prettyPhoto(
                {
                    social_tools: false,
                    deeplinking: false,
                    animation_speed: 'normal', theme: 'pp_default', slideshow: 3000, autoplay_slideshow: false,
                    allow_resize: true, /* Resize the photos bigger than viewport. true/false */
                    default_width: 800, 
                    default_height: 600,
                });
        }
        else {
            $("a[rel^='prettyPhoto']").prettyPhoto(
                {
                    social_tools: false,
                    deeplinking: false,
                    animation_speed: 'normal', theme: 'pp_default', slideshow: 3000, autoplay_slideshow: false,
                    allow_resize: true, /* Resize the photos bigger than viewport. true/false */
                    default_width: widthReview,
                    default_height: heightReview,
                });
        }
        
    }

    return {
        init: init
    };
});
