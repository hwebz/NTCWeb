define(['jquery', 'underscore', 'base/page-scroll-animation', 'base/page-scroll', 'base/count-up'], function ($, _, pageScrollAnimation, pageScroll) {

    'use strict';

    var recordNumber = null;

    function init(container) {
        pageScrollAnimation.init(container);
    }

    return {
        init: function (container) {
            init(container);
        }
    };
});