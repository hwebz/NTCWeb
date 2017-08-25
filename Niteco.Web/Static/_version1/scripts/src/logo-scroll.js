define(['jquery', 'underscore', 'base/modules/animate'], function ($, _, animate) {
    'use strict';

    var fixedLogo = $('.fixed-logo');
    var absoluteLogos = $('.absolute-logo');

    var callbackBeforeScroll = function (currentPanel) {
        fixedLogo.removeClass('header-logo');
    };

    var callbackAfterScroll = function(currentPanel) {
        if (currentPanel <= 1)
            fixedLogo.removeClass('header-logo');
        else
            fixedLogo.addClass('header-logo');
    };
    var setLogo = function () {
        for (var i = 0; i < absoluteLogos.length; i++) {
            absoluteLogos.eq(i).css('top', fixedLogo.offset().top - absoluteLogos.eq(i).closest('.parallax-panel').offset().top);
        }
    };

    return {
        init: function () {
            $(document).on('scroll', function () {
                setLogo();
            });
        },
        
        callbackBeforeScroll: callbackBeforeScroll,
        
        callbackAfterScroll: callbackAfterScroll
    };
});