define(['jquery', 'underscore', 'base/modules/animate', 'lib/owl.carousel'], function ($, _, animate, owl) {
    'use strict';

    var testimonialContainer = $('#testimonialContainer');
    var testimonialList = $('#testimonialContainerSlider');

    return {
        init: function (scrollToTop) {
            if (testimonialList.children().length >= 2) {
                testimonialList.addClass('owl-carousel').owlCarousel({
                    items: 1,
                    smartSpeed: 500,
                    loop: true,
                    nav: true
            });
            }

            $('#testimonialContainer .panel-logo').on('click', function () {
                if (scrollToTop)
                    scrollToTop();
            });
        },
        rollbackAnimation: function () {
            animate(testimonialContainer, 'stop', true);
            testimonialContainer.css('opacity', '0');
        },

        playAnimation: function () {
            var durationAnimate = 250;
            $.when(animate(testimonialContainer, { opacity: 1 }, { duration: durationAnimate }));
        }
    };
});