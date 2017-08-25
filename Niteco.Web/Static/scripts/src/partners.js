define(['jquery', 'underscore', 'base/page-scroll-animation'], function ($, _, pageScrollAnimation) {

    'use strict';

    function init(container) {
        pageScrollAnimation.init(container);
    }

    return {
        init: function (container) {
            init(container);
        }
    };
});
