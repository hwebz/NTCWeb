define(['jquery'], function () {

    'use strict';

    var articleIndex = 1;
    var listArticles = [];
    var isEnded = false;
    var isLoaded = true;

    var getArticle = function (parent, callback) {
        
        var location = window.location + "";

        var firstArticle = parseInt($("#blog-list-articles__content .blog-list-articles__article").first().find('.next-page').text());
        console.log('first article: ' + firstArticle);
        if (!firstArticle) {
            isEnded = true;
            if (callback)
                callback();
            return;
        }

        if (location.indexOf('?') > 0)
            location += "&page=" + articleIndex;
        else
            location += "?page=" + articleIndex;
        
        $.get(location, function (data) {
            console.log(data);
            if (data) {
                parent.append(data);
                setThumbnailBackground(".blog-list-articles__article-thumbnail");
                articleIndex++;
                console.log("next page:" + parseInt($(data).find(".next-page").text()));
                if (!parseInt($(data).find(".next-page").text())) {
                    isEnded = true;
                }
            }

            if (callback)
                callback();
        }).fail(function () {
            isEnded = true;
            console.log("Failed to load new articles of " + location);
        });
    };

    function setThumbnailBackground(thumbnailClasses) {
        var thumbnailWrapper = $(thumbnailClasses);

        thumbnailWrapper.each(function () {
            var imageSrc = $(this).find("img");
            var imageSrcCheck = imageSrc.attr("srcset") != undefined ? imageSrc.attr("srcset") : (imageSrc.attr("src") != undefined ? imageSrc.attr("src") : undefined);
            if (imageSrcCheck != undefined) {
                $(this).css('background-image', 'url(' + imageSrcCheck + ')');
                imageSrc.hide();
            }
        });
    }

    function infiniteScroll(parent) {
        setThumbnailBackground(".blog-list-articles__article-thumbnail");

        var currentTime, lastTime, timeoutScroll;
        $(window).scroll(function () {
            currentTime = new Date().getTime();
            if (!lastTime)
                lastTime = currentTime;

            if (currentTime > lastTime + 200 || !timeoutScroll) {
                if (timeoutScroll)
                    clearTimeout(timeoutScroll);

                lastTime = currentTime;

                timeoutScroll = setTimeout(function () {
                    var parentPosition = parent.offset().top + parent.height() - 550;
                    var firstArticle = parseInt($("#blog-list-articles__content .blog-list-articles__article").first().find('.next-page').text());
                    if ($(window).scrollTop() >= parentPosition && isLoaded && !isEnded) {
                        isLoaded = false;
                        if (firstArticle) {
                            $(".loading-area").fadeIn('fast');
                        }
                        getArticle(parent, function () {
                            isLoaded = true;

                            $(".loading-area").fadeOut('fast');
                        });
                    }
                }, 1000);
            }
        });
    }
    return {
        init: function (container) {
            infiniteScroll($("#blog-list-articles__content"));
        }
    };
});