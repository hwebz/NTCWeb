define(['jquery', 'underscore', 'base/modules/animate', 'base/top-navigator', 'base/request-page', 'templates/search_result'], function ($, _, animate, topNavigator, requestPage, searchResult) {

    'use strict';

    var searchContainer = null;
    var position = null;
    var size = null;
    var searchAction = "";
    var timeoutToSearch = 1000;
    var showMoreUrl = "";
    var searchText = "";
    var oldSearchText = "";

    function init() {
        registerEventShowSearch();

        // search action
        searchAction = searchContainer.attr('data-action-search');
        var timeoutSearch = null;
        searchContainer.find('#searchBox').on('keyup', function (e) {
            if (timeoutSearch && e.which != 13)
                clearTimeout(timeoutSearch);

            searchText = $(this).val();
            if (searchText.trim().length <= 0)
                return;

            if (e.which == 13) {
                if (oldSearchText == searchText)
                    return;
                oldSearchText = searchText;
                search(searchAction + "?q=" + encodeURIComponent(searchText), generateSearchResult);
                return;
            }

            timeoutSearch = setTimeout(function () {
                if (oldSearchText == searchText)
                    return;
                oldSearchText = searchText;
                search(searchAction + "?q=" + encodeURIComponent(searchText), generateSearchResult);
                checkShowMore();
            }, timeoutToSearch);
        });

        searchContainer.find('.search-showmore-container span').on('click', function () {
            checkShowMore(null);
            search(showMoreUrl, generateSearchResult, true);
        });

        /*
        searchContainer.find('.search-default a').on('click', function (e) {
            e.preventDefault();
            var href = $(this).attr('href');
            openALink(href);
            return false;
        });
        */
    }

    function generateSearchResult(data, isShowMore) {
        animate(searchContainer.find('.search-default'), 'transition.slideDownOut', { duration: 300 });
        var result = $(searchResult(data.data));
        if (!isShowMore) {
            searchContainer.find('.search-result').html('');
        }
        searchContainer.find('.search-result').append(result);

        var listSearchItems = result.find('.search-result__item');
        for (var i = 0; i < listSearchItems.length; i++) {
            if (i < listSearchItems.length - 1) {
                animate(listSearchItems.eq(i), 'transition.slideUpIn', { duration: 200, delay: i * 200 });
            } else {
                animate(listSearchItems.eq(i), 'transition.slideUpIn', { duration: 200, delay: i * 200 }).then(function () {
                    checkShowMore(data);
                });
            }
            
            /*
            listSearchItems.eq(i).on('click', function (e) {
                e.preventDefault();
                var href = $(this).attr('href');
                openALink(href);
                return false;
            });
            */
        }
        
        if (!data.data || data.data.length == 0) {
            searchContainer.find('.search-result').html('<div class="search-result__message">' + data.message + '</div>');
        }
        searchContainer.find('#searchBox').blur();
        searchContainer.focus();
    }

    function checkShowMore(data) {
        if (data && data.hasShowMore) {
            showMoreUrl = searchAction + "?q=" + searchText + "&page=" + (data.currentPage + 1);
            searchContainer.find('.search-showmore-container').css('display', 'block');
        } else {
            searchContainer.find('.search-showmore-container').css('display', 'none');
        }
    }

    function openALink(href) {
        closeSearchBox(function () {
            topNavigator.hideMenu();
            requestPage.loadPageFromUrl(href);
        });
    }

    function search(searchUrl, callback, isShowMore) {
        searchContainer.find('.search-form').addClass('searching');
        $.ajax({
            url: searchUrl,
            type: 'GET',
            async: true,
            datatype: 'json',
            success: function (data) {
                searchContainer.find('.search-form').removeClass('searching');
                if (data.success) {
                    if (callback)
                        callback(data, isShowMore);
                }
            },

            error: function () {
            }
        });
    }

    function registerEventShowSearch() {
        var showSearchPanel = _.debounce(function () {
            position = topNavigator.getContainer().find('.top-navigator__search-container').offset();
            size = {
                width: topNavigator.getContainer().find('.top-navigator__search-container').outerWidth(),
                height: topNavigator.getContainer().find('.top-navigator__search-container').outerHeight()
            };

            animate(searchContainer, {
                top: position.top - $(window).scrollTop() + 'px',
                width: size.width + 'px',
                height: size.height + 'px',
                right: $(window).width() - position.left - size.width + 'px',
                opacity: 1,
                position: 'absolute'
            }, {
                duration: 0,
                display: 'block',
                easing: 'ease-in-out'
            }).then(function () {

                searchContainer.addClass('expanding');
                animate(searchContainer, {
                    'padding-right': 0,
                    'top': 0,
                    'right': 0,
                    'width': '100%',
                    'height': '100%'
                }, {
                    duration: 500,
                    easing: 'ease-in-out'
                }).then(function () {
                    searchContainer.addClass('active');
                    animate(searchContainer.find('.search-default'), 'transition.slideUpIn', { duration: 300 });
                    searchContainer.find('#searchBox').focus();
                });

                searchContainer.find('.close-button').one('click', function (e) {
                    closeSearchBox();
                });

            });
        }, 10);

        topNavigator.getContainer().find('.top-navigator__search-container').one('click', function () {
            oldSearchText = "";
            showSearchPanel();
            $(this).find('#top-navigator__search-input').focus();
        });
    }

    function closeSearchBox(callback) {
        searchContainer.removeClass('active');
        searchContainer.find('.search-default').css('display', 'none');
        searchContainer.find('.search-result').html('');
        searchContainer.find('#searchBox').val('');

        var timeout = setTimeout(function () {
            clearTimeout(timeout);

            animate(searchContainer, {
                right: $(window).width() - position.left - size.width + 'px',
                top: position.top - $(window).scrollTop() + 'px',
                width: size.width + 'px',
                height: size.height + 'px'
            }, {
                duration: 500,
                easing: 'ease-in-out',
                display: 'none'
            }).then(function () {
                searchContainer.removeClass('expanding');

                var timeoutRegister = setTimeout(function () {
                    clearTimeout(timeoutRegister);
                    searchContainer.css({
                        width: 0,
                        height: 0,
                        opacity: 0
                    });

                    registerEventShowSearch();
                }, 0);

                if (callback)
                    callback();
            });
        }, 200);
    }

    return {
        init: function (container) {
            searchContainer = container;
            init();
        }
    };
});
