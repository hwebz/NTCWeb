define(['jquery', 'underscore', 'base/modules/animate'], function ($, _, animate) {

    'use strict';

    var loadingContainer = null;

    return {        
        init: function (container) {
            loadingContainer = container;
        },
        
        startLoading: function () {
            animate(loadingContainer, { opacity: 1, duration: 200 }).then(function() {
                loadingContainer.addClass('loading');
            });
        },
        
        completeLoading: function(callback) {
            loadingContainer.removeClass('loading').addClass('animating');
            setTimeout(function() {
                loadingContainer.one('transitionend webkitTransitionEnd oTransitionEnd otransitionend MSTransitionEnd', function () {
                    animate(loadingContainer, { opacity: 0, duration: 500 }).then(function () {
                        loadingContainer.removeClass('animating').removeClass('loading');
                        setTimeout(function () {
                            if (callback)
                                callback();
                        }, 100);
                    });
                });
            },100);
        }

    };
});
