define(['jquery', 'underscore', 'base/page-scroll', 'vendor/mobile-detect'], function ($, _, pageScroll, MobileDetect) {

    'use strict';

    pageScroll.init();

    var checkingItems = ".checking-animate";
    var removeClass = "inactive";
    var heightW = $(window).height();
    var deltaH = 100;

    var md = new MobileDetect(window.navigator.userAgent);
    
    function PageScrollAnimator(container) {
        if (container) {
            var items = container.find(checkingItems);
            for (var i = 0; i < items.length; i++) {
                if (items.eq(i).hasClass(removeClass) && items.eq(i).offset().top < heightW) {
                    items.eq(i).removeClass(removeClass);
                }
            }
            
            pageScroll.addCallback(function(scrollTop) {
                for (var i = 0; i < items.length; i++) {
                    if (items.eq(i).hasClass(removeClass) && items.eq(i).offset().top < scrollTop + heightW - deltaH) {
                        items.eq(i).removeClass(removeClass);
                    }
                }
            });
        }
    }

    return {        
        init: function (container) {
            if (!md.mobile())
                return new PageScrollAnimator(container);
            else {
                var items = container.find(checkingItems);
                for (var i = 0; i < items.length; i++) {
                    if (items.eq(i).hasClass(removeClass)) {
                        items.eq(i).removeClass(removeClass);
                    }
                }
            }
        }
    };
});
