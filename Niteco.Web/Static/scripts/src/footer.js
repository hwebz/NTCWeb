define(['jquery', 'underscore', 'vendor/mobile-detect'], function ($, _, MobileDetect) {

    'use strict';
    
    var md = new MobileDetect(window.navigator.userAgent);

    function init(container) {
        if (md.mobile()) {
            container.addClass('normal-footer');
            container.parent().css('padding-bottom', 0);
        }
    }

    return {
        init: init
    };
});
