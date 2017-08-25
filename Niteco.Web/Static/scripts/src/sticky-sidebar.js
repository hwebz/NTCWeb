define(['jquery'], function () {
    'use strict';

    (function ($) {

        $.fn.stickySidebar = function (options) {

            if ($(this)) {
                var config = $.extend({
                    container: '#container',
                    sidebarTopMargin: 0
                }, options);

                var stickyObj = {
                    width: $(this).width(),
                    height: $(this).height(),
                    top: $(this).offset().top,
                    left: $(this).offset().left
                };

                if ($(window).width() < 1183) {
                    config.sidebarTopMargin = 80;
                }

                if ($(config.container).offset() != undefined) {
                    var container = {
                        width: $(config.container).width(),
                        height: $(config.container).height(),
                        top: $(config.container).offset().top,
                        left: $(config.container).offset().left
                    };
                } else {
                    var container = {};
                }

                var fixSidebr = function () {
                    if ($(config.container).offset() != undefined) {
                        container = {
                            width: $(config.container).width(),
                            height: $(config.container).height(),
                            top: $(config.container).offset().top,
                            left: $(config.container).offset().left
                        };
                    }
                    var objPosition = ($(window).scrollTop() - (stickyObj.top - config.sidebarTopMargin));
                    var containerBottom = container.height + container.top - (config.sidebarTopMargin + 100);
                    var currentScroll = ($(window).scrollTop() + stickyObj.height);
                    var distance = ($(window).scrollTop() + config.sidebarTopMargin) - stickyObj.top;
                    $(this).attr('distance', distance);

                    if ($(window).width() < 1183) {
                        containerBottom -= 20;
                    }

                    if (objPosition > 0) {
                        if (!$(this).hasClass('sticky-sidebar') && !(currentScroll > containerBottom)) {
                            $(this)
							.addClass('sticky-sidebar')
							.css({
							    width: stickyObj.width,
							    height: stickyObj.height,
							    top: config.sidebarTopMargin,
							    left: stickyObj.left
							});
                        } else {
                            if (currentScroll > containerBottom) {
                                $(this).css({
                                    top: config.sidebarTopMargin - (currentScroll - containerBottom)
                                });
                                $(this).attr('distance', distance - (config.sidebarTopMargin - (config.sidebarTopMargin - (currentScroll - containerBottom))));
                                $(this).attr('top-distance', (config.sidebarTopMargin - (currentScroll - containerBottom)));
                            } else {
                                $(this).removeAttr('top-distance');
                                $(this).css({
                                    top: config.sidebarTopMargin
                                });
                            }
                        }
                    } else {
                        $(this).removeClass('sticky-sidebar').removeAttr('style');
                    }

                };

                function updateParams(selector, containerParent) {
                    selector.removeClass('sticky-sidebar').removeAttr('style');
                    stickyObj = {
                        width: selector.width(),
                        height: selector.height(),
                        top: selector.offset().top,
                        left: selector.offset().left
                    }
                    if ($(window).width() < 1183) {
                        config.sidebarTopMargin = 80;
                    }

                    container = {
                        width: $(containerParent).width(),
                        height: $(containerParent).height(),
                        top: $(containerParent).offset().top,
                        left: $(containerParent).offset().left
                    }
                    setTimeout(function () {
                        fixSidebr();
                    }, 300);
                }

                return this.each(function () {
                    $(window).on('scroll', $.proxy(fixSidebr, this));
                    $(window).on('resize', $.proxy(function () {
                        updateParams($(this), config.container);
                        setTimeout(function () {
                            if ($(this).offset() != undefined) {
                                updateParams($(this), config.container);
                            }
                        }, 500);
                    }, this));
                    $(window).on("orientationchange", $.proxy(function () {
                        updateParams($(this), config.container);
                        setTimeout(function () {
                            if ($(this).offset() != undefined) {
                                updateParams($(this), config.container);
                            }
                        }, 500);
                    }, this));
                    $.proxy(fixSidebr, this)();
                    if (config.container.indexOf("blog-detail-content__container") != -1) {
                        $('#disqus_thread').bind("DOMSubtreeModified", $.proxy(function () {
                            updateParams($(this), config.container);
                            setTimeout(function () {
                                if ($(this).offset() != undefined) {
                                    updateParams($(this), config.container);
                                }
                            }, 500);
                        }, this));
                    } else {
                        $('#blog-list-articles__content').bind("DOMSubtreeModified", $.proxy(function () {
                            updateParams($(this), config.container);
                            setTimeout(function () {
                                if ($(this).offset() != undefined) {
                                    updateParams($(this), config.container);
                                }
                            }, 500);
                        }, this));
                    }
                });
            }
        };
    })(jQuery, window);

    function initialize() {
        if ($('#blog-list-sidebar__widget_hiring')) {
            $('#blog-list-sidebar__widget_hiring').stickySidebar({
                container: $('.blog-detail-content__container').length > 0 ? '.blog-detail-content__container > .container' : '.blog-list-content__container > .container',
                sidebarTopMargin: 110
            });

            $(window).on('resize', function () {
                $('#blog-list-sidebar__widget_hiring').removeClass('sticky-sidebar').removeAttr('style');
                $('#blog-list-sidebar__widget_hiring').stickySidebar({
                    container: '.blog-list-content__container > .container',
                    sidebarTopMargin: 110
                });
            });
        }
    }

    return {
        init: function (container) {
            return new initialize();
        }
    }
});