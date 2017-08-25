define(['jquery', 'underscore', 'base/modules/animate'], function ($, _, animate) {
    'use strict';

    var listImages = $('#aboutContainer .aboutUs-image-container');
    var listTexts = $('#aboutContainer .aboutUs-content-container');
    
    return {
        init: function (scrollToTop) {
            $('#aboutContainer .panel-logo').on('click', function () {
                if (scrollToTop)
                    scrollToTop();
            });
        },
        
        rollbackAnimation: function () {
            animate(listImages.eq(0), 'stop', true);
            animate(listImages.eq(1), 'stop', true);
            animate(listTexts.eq(0), 'stop', true);
            animate(listTexts.eq(1), 'stop', true);
            
            listImages.css('opacity', '0');
            listTexts.css('opacity', '0');
        },
        
        playAnimation: function () {
            var duration = 500;
            $.when(animate(listImages.eq(0), 'transition.slideLeftBigIn', { duration: duration }),
                animate(listTexts.eq(0), 'transition.slideRightBigIn', { duration: duration })
            ).then(function() {
                animate(listImages.eq(1), 'transition.slideUpBigIn', { duration: duration });
                animate(listTexts.eq(1), 'transition.slideUpBigIn', { duration: duration });
            });

        }
    };
});