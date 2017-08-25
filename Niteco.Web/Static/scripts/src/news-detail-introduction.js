define(['jquery', 'underscore', 'base/modules/animate', 'base/page-scroll', 'vendor/mobile-detect', 'lib/picturefill'], function ($, _, animate, pageScroll, MobileDetect) {

    'use strict';

    pageScroll.init();

    var heightPageHeader = 35; // percent
    var contentHeight = 0;
    var isInit = false;
    var md = new MobileDetect(window.navigator.userAgent);

    function calculateHeight(container) {
        if (!isInit) {
            isInit = true;

            if (!window.parent.$('iframe') || window.parent.$('iframe').length == 0 || !isInEditMode) {
                var newHeight = $(window).height() * heightPageHeader / 100;
                var w = $(window).width();
                var tempH = w;
                if (w >= 992) {
                    tempH = w * 0.5047;
                }
                else
                    if (w >= 768) {
                        tempH = w * 1.4;
                    }
                    else {
                        tempH = w * 1.8;
                    }

                if (newHeight > tempH) {
                    newHeight = Math.round(tempH - 1);
                }

                //container.parent().height(newHeight + 'px');
                //$('head').append('<style type="text/css">.news-detail-introduction-container {height:' + newHeight + 'px' + ';}</style>');
            }

            var background = container.find('.news-detail-introduction__background');
            contentHeight = background.height();
        }

        window.picturefill();
    }

    function PageIntroduction(container) {
        calculateHeight(container);

        var content = container.find('.news-detail-introduction__content');
        var background = container.find('.news-detail-introduction__background');
        contentHeight = background.height();

        var callbackScroll = _.debounce(function (scrollTop) {
            if (scrollTop > contentHeight)
                return;

            background.css('top', (scrollTop / 2) + 'px');
            content.css({
                'margin-top': (scrollTop / 2) + 'px',
                'opacity': (1 - scrollTop * 1.5 / contentHeight)
            });
        }, 0);

        if (!md.mobile()) {
            pageScroll.addCallback(callbackScroll);
        }

        $(window).on('resize', function () {
            var timeout = setTimeout(function () {
                isInit = false;
                calculateHeight(container);
                clearTimeout(timeout);
            }, 500);
        });
    }

    return {
        init: function (container) {
            var pageIntroBackground = $(".page-introduction-top-block__background img");
            if (pageIntroBackground != undefined && PageIntroduction != null && pageIntroBackground.length > 0) {
                if (pageIntroBackground.attr("src").indexOf(".") != -1) {
                    pageIntroBackground.css({ 'display': 'inline-block' });
                }
            }
            return new PageIntroduction(container);
        }
    };
});
