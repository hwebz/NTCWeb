define(['jquery', 'underscore', 'base/page-scroll-animation'], function ($, _, pageScrollAnimation) {

    'use strict';

    function init(container) {
        pageScrollAnimation.init(container);
    }

    function initClickOnListServicePreview() {
        $(".services-item-preview ").each(function () {
            $(this).on('click', function () {
                var dataId = $(this).attr("data-id");
                if (dataId == 7) {
                    var scrollTarget = $(".service-technology__container[data-id='fullInfo-" + dataId + "']");
                    if (scrollTarget == null || scrollTarget.length == 0) {
                        scrollTarget = $(".list-service-detail .service-item-detail__container[data-id='fullInfo-" + dataId + "']");
                    }

                    if (scrollTarget != null && scrollTarget.length > 0) {
                        $("html, body").animate({
                            scrollTop: (scrollTarget.offset().top - 85) + 'px'
                        }, 600);
                    }
                } else {
                    $("html, body").animate({
                        scrollTop: ($(".list-service-detail .service-item-detail__container[data-id='fullInfo-" + dataId + "']").offset().top - 85) + 'px'
                    }, 600);
                }
            });
        });
    }

    return {
        init: function (container) {
            init(container);
            initClickOnListServicePreview();
        }
    };
});
