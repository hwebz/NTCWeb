define(['jquery', 'underscore', 'base/modules/animate'], function ($, _, animate) {
    'use strict';

    var header = $('#contactContainer .block-header-contain');
    var listOptions = $('#contactContainer .certificate-item');
    var listLocations = $('#contactContainer .contact-location-item');

    return {
        init: function (scrollToTop) {
            $('#contactContainer .panel-logo').on('click', function () {
                if (scrollToTop)
                    scrollToTop();
            });
        },
        
        rollbackAnimation: function () {
            animate(header, 'stop', true);
            animate(listOptions.eq(0), 'stop', true);
            animate(listOptions.eq(1), 'stop', true);
            animate(listOptions.eq(2), 'stop', true);
            animate(listLocations.eq(0), 'stop', true);
            animate(listLocations.eq(1), 'stop', true);
            animate(listLocations.eq(2), 'stop', true);
            animate(listLocations.eq(3), 'stop', true);
            animate(listLocations.eq(4), 'stop', true);


            header.css('opacity', '0');
            listOptions.css('opacity', '0');
            listLocations.css('opacity', '0');
        },
        
        playAnimation: function () {
            var durationHeader = 250;
            var durationOption = 250;
            var durationLocation = 250;
            $.when(animate(header, { opacity: 1 }, { duration: durationHeader }))
                .then(function () {
                    return animate(listOptions.eq(1), { opacity: 1 }, { duration: durationOption });
                }).then(function () {
                    return $.when(animate(listOptions.eq(0), 'transition.slideLeftBigIn', { duration: durationOption }),
                                animate(listOptions.eq(2), 'transition.slideRightBigIn', { duration: durationOption }));
                }).then(function () {
                    return animate(listLocations.eq(0), 'transition.slideUpIn', { duration: durationLocation });
                }).then(function () {
                    return animate(listLocations.eq(1), 'transition.slideUpIn', { duration: durationLocation });
                }).then(function () {
                    return animate(listLocations.eq(2), 'transition.slideUpIn', { duration: durationLocation });
                }).then(function () {
                    return animate(listLocations.eq(3), 'transition.slideUpIn', { duration: durationLocation });
                }).then(function () {
                    return animate(listLocations.eq(4), 'transition.slideUpIn', { duration: durationLocation });
                });
        }
    };
});