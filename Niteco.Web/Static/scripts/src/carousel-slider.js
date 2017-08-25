define(['jquery', 'underscore', 'base/modules/animate', 'lib/owl.carousel'], function ($, _, animate, owl) {

    'use strict';

    function CarouselSlider(container) {
        if (container.children().children().length >= 2) {
            container.children().addClass('owl-carousel').owlCarousel({
                items: 1,
                smartSpeed: 500,
                loop: true,
                nav: true
            });
        }
    }

    return {
        init: function (container) {
            return new CarouselSlider(container);
        }
    };
});
