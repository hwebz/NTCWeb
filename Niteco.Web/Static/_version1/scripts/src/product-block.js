define(['jquery', 'underscore', 'base/modules/animate', 'lib/owl.carousel'], function ($, _, animate, owl) {
    'use strict';

    var productContainer = $('#productContainer');
    var productText = $('#productContainer .in-print-col');
    var productImage = $('#productContainer .in-print-col-right');
    var productSliderContainer = $('#productSliderContainer');
    var firstTime = true;

    return {
        init: function (scrollToTop) {
            $('#productContainer .panel-logo').on('click', function () {
                if (scrollToTop)
                    scrollToTop();
            });
            
            if (productSliderContainer.children().length >= 2) {
                productSliderContainer.addClass('owl-carousel').owlCarousel({
                    items: 1,
                    smartSpeed: 500,
                    loop: true
                });
            }
        },
        
        rollbackAnimation: function () {
            animate(productContainer, 'stop', true);
            animate(productText, 'stop', true);
            animate(productImage, 'stop', true);
            
            productContainer.css('opacity', '0');
            productText.css('opacity', '0');
            productImage.css('opacity', '0');
        },
        
        playAnimation: function () {
            var durationContainer = 250;
            var durationText = 500;
            var durationImage = 500;

            animate(productContainer, { opacity: 1 }, { duration: durationContainer, delay: 0 }).then(function() {
                $.when(animate(productText, 'transition.slideDownBigIn', { duration: durationText, delay: 200 }),
                    animate(productImage, 'transition.slideUpBigIn', { duration: durationImage, delay: 200 }))
                    .then(function() {
                        if (firstTime) {
                            firstTime = false;
                            productContainer.removeClass('animation-object');
                            productContainer.find('.animation-object').removeClass('animation-object');
                        }
                    });
            });
        }
    };
});
