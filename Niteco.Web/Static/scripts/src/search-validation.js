define(['jquery'], function () {
    'use strict';

    var isCutted = false;

    function initialize() {

        $(".blog-list-sidebar__widget_search-form #blog-list-sidebar__widget_search-form__input-field").keypress(function () {
            if ($(this).val() != "") {
                $(this).removeClass('error');
            }
        });

        $(".blog-list-sidebar__widget_search-form form").submit(function () {
            var searchField = $(this).find("#blog-list-sidebar__widget_search-form__input-field");

            if (searchField.val() == "") {
                searchField.addClass('error');
                return false;
            }
        });
    }

    function restructureCommentText(obj) {
        var txt = parseInt(obj.text());
        if ($(window).width() < 480) {
            obj.html(txt);

            obj.unbind();
        } else {
            if (isNaN(txt)) {
                obj.html("0 Comments");
            } else {
                obj.html(txt + (txt == 1 ? " Comment" : " Comments"));
            }
        }
    }

    function setCommentText() {
        //$(".blog-list-articles__article-meta__comment").each(function () {
        //    $(this).bind("DOMSubtreeModified", function () {
        //        restructureCommentText($(this));
        //    });
        //});

        var checkCommentsTextOnLoad = setInterval(function () {
            if ($(window).width() <= 480) {
                if ($(".blog-list-articles__article-meta__comment") && $(".blog-list-articles__article-meta__comment")[0].innerHTML.indexOf('Comment') != -1) {
                    $(".blog-list-articles__article-meta__comment").each(function () {
                        restructureCommentText($(this));
                    });
                } else {
                    clearInterval(checkCommentsTextOnLoad);
                }
            }
        }, 500);

        $(window).on("resize orientationchange scroll", function () {
            if (!isCutted) {
                isCutted = true;
                setTimeout(function () {
                    $(".blog-list-articles__article-meta__comment").each(function () {
                        restructureCommentText($(this));
                    });
                    isCutted = false;
                }, 500);
            }
        });

        //$("#blog-list-articles__content").unbind().bind("DOMSubtreeModified", function () {
        //    if (!isCutted) {
        //        isCutted = true;
        //        setTimeout(function () {
        //            $(".blog-list-articles__article-meta__comment").each(function () {
        //                $(this).bind("DOMSubtreeModified", function () {
        //                    var txt = parseInt($(this).text());
        //                    if ($(window).width() < 480) {
        //                        $(this).html(txt);

        //                        $(this).unbind();
        //                    } else {
        //                        $(this).html(txt + (txt == 1 ? " Comment" : " Comments"));
        //                    }
        //                });
        //            });
        //            isCutted = false;
        //        }, 500);
        //    }
        //});
    }

    return {
        init: function (container) {
            setCommentText();
            return new initialize();
        }
    }
});