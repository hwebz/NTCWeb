define(['jquery'], function () {

    'use strict';

    var articleIndex = 1;
    var listArticles = [];
    var isEnded = false;
    var isLoaded = true;

    var getArticle = function (parent) {
        var location = window.location + "";
        if (location.endsWith("#")) location.replace("#", "");
        var firstArticle = parseInt($("#news-list-articles__content .news-list-article").first().find('.next-page').text());
        if (location.indexOf('?') > 0)
            location += "&page=" + articleIndex;
        else
            location += "?page=" + articleIndex;
        if (!isEnded) {
            $.get(location, function (data) {
                if (!firstArticle) {
                    isEnded = true;
                    return;
                }
                if (data != undefined && data != "" && data != null) {
                    parent.append(data);
                    defineSummaryTextLength(220);
                    setThumbnailBackground(".news-list-article__thumbnail-box");
                    articleIndex++;

                    if (!parseInt($(data).find(".next-page").text())) {
                        isEnded = true;
                    }
					
					reArrangement();
                }
            }).fail(function () {
                isEnded = true;
                console.log("Failed to load new articles of " + location);
            });
        }
    };

    var reArrangement = function () {
        $(".mobile-article-format").remove();
        if ($(window).width() < 768) {
            var type1 = $(".news-list-article-type-1");
            type1.hide();
            type1.each(function () {
                var _that = $(this);
                var title = _that.find(".news-list-article__title a").text(),
                    desc = _that.find(".news-list-article__summary-desc p").text(),
                    readmore = _that.find(".news-list-article__read-more").text(),
                    readmorelink = _that.find(".news-list-article__read-more").attr("href"),
                    thumbnail = _that.find(".news-list-article__thumbnail-box img").attr("src").toString(),
                    thumbnailLink = _that.find(".news-list-article__thumbnail >a").attr('href').toString(),
                    date = {
                        day: _that.find(".news-list-article__meta-date").text(),
                        monthYear: _that.find(".news-list-article__meta-monthYear").text()
                    };

                    var newFormat = '<div class="row news-list-article news-list-article-type-2 mobile-article-format"><div class="col-xs-12 col-sm-6 col-md-6 news-list-article__content"><h2 class="news-list-article__title"><a href="#">' + title + '</a></h2><div class="news-list-article__summary-desc"><p>' + desc + '</p></div><a class="news-list-article__read-more" href="' + readmorelink + '">' + readmore + '</a></div><div class="col-xs-12 col-sm-6 col-md-6 news-list-article__thumbnail"><a href="' + thumbnailLink + '"><div class="news-list-article__thumbnail-box"><picture><source srcset="' + thumbnail + '" media="(max-width: 767px)"><img src="' + thumbnail + '"></picture><div class="news-list-article__meta"><span class="news-list-article__meta-date">' + date.day + '</span><span class="news-list-article__meta-monthYear">' + date.monthYear + '</span></div></div></a></div></div>';
                $(newFormat).insertAfter(_that);
            });
        } else {
            var type1 = $(".news-list-article-type-1");
            type1.show();
        }
        //defineSummaryTextLength(220);
        calculateHeightOfText();
        setThumbnailBackground(".news-list-article__thumbnail-box");
    }

    function defineSummaryTextLength(length) {
        var summaryText = $(".news-list-article__summary-desc p");
        summaryText.each(function () {
            var txt = $(this).text();
            if (txt.length >= length) {
                txt = txt.substr(0, length);
                txt = txt.substr(0, txt.lastIndexOf(" "));
                $(this).text(txt + "...");
            }
        });
    }

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

    function calculateHeightOfText() {
        var article = $(".news-list-article");
        if ($(window).width() >= 768) {
            article.find(".news-list-article__summary-desc").removeAttr("style");
            article.each(function () {
                var thumbnailHeight = $(this).find(".news-list-article__thumbnail").height();
                var content = $(this).find(".news-list-article__content");
                var realContentHeight = content.height();
                var contentHeight = realContentHeight + 120;
                var titleHeight = content.find(".news-list-article__title").height() + 50;
                var summaryDescEle = content.find(".news-list-article__summary-desc");
                var summaryDescHeight = summaryDescEle.height();
                var newSummaryDescHeight = (contentHeight - thumbnailHeight > 0) ? parseInt(((realContentHeight - (contentHeight - thumbnailHeight)) - titleHeight) / 30, 10) * 30 : -1;
                if (!isNaN(newSummaryDescHeight) && newSummaryDescHeight > 0) {
                    summaryDescEle.css("height", newSummaryDescHeight + "px");
                }
            });
        } else {
            article.find(".news-list-article__summary-desc").removeAttr("style");
        }
    }

    return {
        infiniteScroll: function (parent) {
            //defineSummaryTextLength(220);
            setThumbnailBackground(".news-list-article__thumbnail-box");
            calculateHeightOfText();
            $(window).scroll(function () {
                var parentPosition = parent.offset().top + parent.height() - 550;
                var firstArticle = parseInt($("#news-list-articles__content .news-list-article").first().find('.next-page').text());
                if ($(window).scrollTop() >= parentPosition && isLoaded && !isEnded) {
                    isLoaded = false;
                    if (!isEnded && firstArticle) {
                        $(".loading-area").fadeIn('fast');
                    }
                    setTimeout(function () {
                        getArticle(parent);
                        reArrangement();
                        isLoaded = true;
                    }, 1000);
                    $(".loading-area").delay(1000).fadeOut('fast');
                }
            });

            reArrangement();

            $(window).resize(function () {
                reArrangement();
                calculateHeightOfText();
            });

            $(window).on("orientationchange", function () {
                reArrangement();
                calculateHeightOfText();
            });
        },
        init: function (container) {
            return new this.infiniteScroll($("#news-list-articles__content"));
        }
    };

});