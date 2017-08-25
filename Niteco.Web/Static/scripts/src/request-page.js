define(['jquery', 'underscore', 'base/loading', 'base/modules/jitRequire', 'vendor/history', 'base/top-navigator', 'base/modules/animate'], function ($, _, loading, jitRequire, history, topNavigator, animate) {

    'use strict';

    var isLoading = false;
    var currentUrl = "";
    var pageWrapper = $('.page-wrapper');
    var stateIndex = 1;
    var callbackHistory = null;
    var isLoadPageByStateChange = false;
    registerHistory();

    function registerHistory() {
        (function (window) {
            History.Adapter.bind(window, 'statechange', function () {
                var state = History.getState();
                var isSameUrl = true;
                if (state.url.length >= currentUrl.length) {
                    if (state.url.lastIndexOf(currentUrl) != state.url.length - currentUrl.length) {
                        isSameUrl = false;
                    }
                } else {
                    if (currentUrl.lastIndexOf(state.url) != currentUrl.length - state.url.length) {
                        isSameUrl = false;
                    }
                }
                if (!isSameUrl) {
                    isLoadPageByStateChange = true;
                    loadPageByHref(state.url, null, function (href) {
                        isLoadPageByStateChange = false;
                        if (callbackHistory)
                            callbackHistory(href);
                    });
                }
            });
        })(window);
    }

    function loadPage(href, callback) {
        showLoading();
        try {
            mapIsCreated = false;
            isAddZoomControl = false;
            $.ajax({
                url: href,
                type: 'GET',
                async: true,
                success: function (data) {
                    hideLoading(function () {
                        displayLoadedPage(data, function (title) {
                            if (!isLoadPageByStateChange) {
                                History.pushState({ state: stateIndex++ }, title, href);
                            }

                            isLoading = false;
                            if (callback)
                                callback(href);
                        });
                    });
                },
                error: function () {
                    hideLoading();
                    displayError();
                }
            });
        }
        catch (ex) {
            hideLoading();
            displayError();
        }
    }

    function showLoading() {
        loading.startLoading();
    }

    function hideLoading(callback) {
        loading.completeLoading(function () {
            if (callback)
                callback();
        });
    }

    function displayLoadedPage(data, callback) {
        var content = $(data).find('.content-container');
        var matches = data.match(/<title>(.*?)<\/title>/);
        var title = matches[1];

        var isBackHome = false;
        if (content.hasClass('home-container')) {
            isBackHome = true;
            content.addClass('loading-back');
        } else {
            content.addClass('loading');
        }
        //pageWrapper.empty();
        pageWrapper.append(content);

        var displayPage = function() {
            var timeout = setTimeout(function() {
                clearTimeout(timeout);

                var listContents = pageWrapper.find('.content-container');
                if (isBackHome) {
                    listContents.addClass('animating-back');
                } else {
                    listContents.addClass('animating');
                }

                listContents.eq(0).one('transitionend webkitTransitionEnd oTransitionEnd otransitionend MSTransitionEnd', function() {
                    for (var i = 0; i < listContents.length; i++) {
                        if (isBackHome) {
                            if (!listContents.eq(i).hasClass('loading-back')) {
                                listContents.eq(i).remove();
                            }
                        } else {
                            if (!listContents.eq(i).hasClass('loading')) {
                                listContents.eq(i).remove();
                            }
                        }
                    }


                    var timeout1 = setTimeout(function() {
                        clearTimeout(timeout1);
                        pageWrapper.find('.content-container').removeClass('loading loading-back animating animating-back');
                        jitRequire.findDeps(pageWrapper, function() {
                            var timeout2 = setTimeout(function() {
                                clearTimeout(timeout2);

                                if (callback)
                                    callback(title);
                            }, 500);
                        });
                    }, 250);
                });
            }, 500);
        };

        displayPage();
    }

    function displayError() {

    }

    function validateUrl(href) {
        if (href && href != "#" && href != currentUrl) {
            currentUrl = getFullUrl(href);
            return true;
        }
        return false;
    }
    
    function getFullUrl(href) {
        if (href.indexOf(window.location.host) < 0) {
            return window.location.host + href;
        }

        return href;
    }
    
    function urlFunction(href) {
        if (href.indexOf('mailto') == 0)
            return true;

        return false;
    }

    function loadPageByHref(href, jqueryObj, callback) {
        if (isLoading) {
            return false;
        }

        if (validateUrl(href)) {
            isLoading = true;
            loadPage(href, callback);

            if (jqueryObj)
                jqueryObj.data('registed', 'true');
        }

        return false;
    }

    function registRequestByContainer(container, callback) {
        var items = container.find('a');
        registRequestByItems(items.toArray().map(function (e) { return $(e); }), callback);
    }

    function unregistRequestByContainer(container) {
        var items = container.find('a');
        unregistRequestByItems(items);
    }

    function registRequestByItems(items, callback) {
        for (var index in items) {
            if (!items[index].data('registed')) {
                items[index].data('registed', "true");
                items[index].on('click', function (e) {
                    e.preventDefault();
                    var href = $(this).attr('href');
                    
                    if (urlFunction(href)) {
                        window.location.href = href;
                    } else if (this.host == location.host) {
                        if ($(this).hasClass('redirect')) {
                            window.open(href, "_blank");
                        } else {
                            loadPageByHref(href, $(this), callback);
                        }
                    } else {
                        window.open(href, "_blank");
                    }
                });
            }
        }
    }

    function unregistRequestByItems(items) {
        for (var index in items) {
            if (items[index].data('registed')) {
                items[index].off('click');
            }
        }
    }

    return {
        registRequestByContainer: registRequestByContainer,

        unregistRequestByContainer: unregistRequestByContainer,

        registRequestByItems: registRequestByItems,

        unregistRequestByItems: unregistRequestByItems,

        loadPageFromUrl: function (href) {
            loadPageByHref(href);
        },

        registerCallbackHistoryChange: function (callback) {
            callbackHistory = callback;
        }
    };
});
