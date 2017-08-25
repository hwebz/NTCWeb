define(['jquery'], function () {

    'use strict';

    function setThumbnailBackground(thumbnailClasses) {
        var thumbnailWrapper = $(thumbnailClasses);

        thumbnailWrapper.each(function () {
            var imageSrc = $(this).find("img");
            var imageSrcCheck = imageSrc.attr("srcset") != undefined ? imageSrc.attr("srcset") : (imageSrc.attr("src") != undefined ? imageSrc.attr("src") : undefined);
            if (imageSrcCheck != undefined) {
                if ($(window).width() >= 768) {
                    $(this).css('background-image', 'url(' + imageSrcCheck + ')');
                    imageSrc.hide();
                } else {
                    $(this).removeAttr("style");
                    imageSrc.show();
                }
            }
        });
    }

    return {
        autoScaleThumbnail: function () {
            setThumbnailBackground(".news-detail-releated-articles__article-thumbnail");
            $(window).resize(function () {
                 setThumbnailBackground(".news-detail-releated-articles__article-thumbnail");
            });

            $(window).on("orientationchange", function () {
                setThumbnailBackground(".news-detail-releated-articles__article-thumbnail");
            });
        },
        init: function (container) {
            return new this.autoScaleThumbnail();
        }
    };

});