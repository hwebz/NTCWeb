define(['jquery', 'underscore', 'base/page-scroll-animation', 'base/page-scroll','base/count-up'], function ($, _, pageScrollAnimation, pageScroll) {

    'use strict';
    
    var recordNumber = null;

    function init(container) {
        pageScrollAnimation.init(container);
        
        var heightW = $(window).height();
        var deltaH = 200;
        recordNumber = container.find(".record-number");

        pageScroll.addCallback(function (scrollTop) {
            if (!recordNumber.hasClass("counted") && (recordNumber.find(".container").offset().top < scrollTop + heightW - deltaH)) {
                recordNumber.addClass("counted");
                countRecord();
            }
        });
    }
    
    function countRecord() {
        recordNumber.find(".statistic-number").each(function () {
            $(this).countTo({
                from: 0,
                to: $(this).attr("data-count"),
                speed: 2000,
                refreshInterval: 100,
                onComplete: function (value) {
                }
            });
        });
    }
    
    return {
        init: function (container) {
            init(container);
        }
    };
});