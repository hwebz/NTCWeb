define(['jquery', 'underscore', 'base/modules/animate', 'base/request-page', 'base/page-scroll'], function ($, _, animate, requestPage, pageScroll) {

    'use strict';

    pageScroll.init();

    var menuContainer = null;
    var isMenuShowed = false;
    var enableToggleMenu = true;

    function init() {
        registerEvent();
    }
    /*
    function generateEmail() {
        var coded = "rN0p@Nr9tPp.PpA";
        var key = "gxR95JMUZkTbiXpf3c1QOCBnjhurEH0KPmVeGzNDF6ys8qvwA7tadWSLI4Yl2o";
        var shift = coded.length;
        var link = "";
        var ltr = "";
        for (var i = 0; i < coded.length; i++) {
            if (key.indexOf(coded.charAt(i)) == -1) {
                ltr = coded.charAt(i);
                link += (ltr);
            }
            else {
                ltr = (key.indexOf(coded.charAt(i)) - shift + key.length) % key.length;
                link += (key.charAt(ltr));
            }
        }
        
        menuContainer.find('.top-navigator__mail').html('info@niteco.com').attr('href', 'mailto:' + link);
    }
    */
    function registerEvent() {

        var showMenuCall = _.debounce(function (event) {
            showMenu(event.target);
        }, 50);
        menuContainer.find('.hamburger-menu__button').on('click', showMenuCall);
        menuContainer.find('a').on('click', hideMenu);

        // Change page when clicking on menu item
        requestPage.registRequestByContainer(menuContainer, function (href) {
            selectMenu(href);
        });

        // Change page when change history
        requestPage.registerCallbackHistoryChange(function (href) {
            selectMenu(href);
            hideMenu();
        });

        // Hide | Show top navigator
        var scrollTopToDisplay = $(window).height() * 70 / 100;
        var absolutePosition = 'absolute';
        var fixedPosition = "fixed";
        var currentPosition = absolutePosition;

        var callbackScroll = _.debounce(function (scrollTop) {
            if (scrollTop > scrollTopToDisplay && currentPosition == fixedPosition)
                return;

            if (scrollTop > scrollTopToDisplay && currentPosition == absolutePosition) {
                currentPosition = fixedPosition;
                menuContainer.css({
                    'position': 'fixed',
                    'top': -menuContainer.height() + 'px'
                });
                menuContainer.addClass('scroll-menu');

                animate(menuContainer, { top: 0 }, { duration: 200, easing: 'easeOutQuad' });
            } else if (scrollTop <= scrollTopToDisplay && currentPosition == fixedPosition) {
                currentPosition = absolutePosition;
                animate(menuContainer, { top: -menuContainer.height() + 'px' }, { duration: 200, easing: 'easeOutQuad' }).then(function () {
                    menuContainer.css({
                        'position': 'absolute',
                        'top': '0'
                    });
                    menuContainer.removeClass('scroll-menu');
                });
            }
        }, 0);

        pageScroll.addCallback(function (scrollTop) {
            callbackScroll(scrollTop);
        });
    }

    function showMenu(target) {
        if (!enableToggleMenu)
            return;

        enableToggleMenu = false;
        isMenuShowed = !isMenuShowed;


        if (isMenuShowed) {
            var bodyMask = $(target).parents('body').find('.content-wrapper .content-wrapper-mask');
            if (!bodyMask || bodyMask.length == 0) {
                $(target).parents('body').find('.content-wrapper').append('<div class="content-wrapper-mask"></div>');
            }
            $(target).parents('body').find('.content-wrapper-mask').on('click', hideMenu);
            stickySidebar(true);
        } else {
            stickySidebar(false);
        }

        $(target).parents('body').find('.content-wrapper').toggleClass('show-menu');

        var callbackEnTransition = _.debounce(function () {
            enableToggleMenu = true;
        }, 10);

        $(target).parents('.top-navigator-container').one('transitionend webkitTransitionEnd oTransitionEnd otransitionend MSTransitionEnd', function () {
            callbackEnTransition();
        });

        $(target).parents('.top-navigator-container').toggleClass('is-active');
        $('body').toggleClass('overflow-hidden');
    }

    function stickySidebar(showed) {
        if ($("#blog-list-sidebar__widget_hiring")) {
            var _this = $("#blog-list-sidebar__widget_hiring");
            if (_this.hasClass('sticky-sidebar')) {
                if (showed) {
                    _this.css({ top: 'inherit', marginTop: parseInt(_this.attr('distance')) + 'px' });
                } else {
                    _this.fadeOut('fast').css({ marginTop: '0px', top: (parseInt(_this.attr('top-distance')) < 0 ? parseInt(_this.attr('top-distance')) : 110) + 'px' }).fadeIn('slow');
                }
            }
        }
    }

    function getFullUrl(href) {
        if (href.indexOf(window.location.host) < 0) {
            return window.location.host + href;
        }

        return href;
    }

    function selectMenu(href) {
        var listItems = menuContainer.find('.top-navigator__menu-container').find('a');
        var currentPage = getFullUrl(href);
        for (var i = 0; i < listItems.length; i++) {
            var menuHref = getFullUrl(listItems.eq(i).attr('href'));
            if (menuHref == currentPage) {
                listItems.eq(i).addClass('selected-item');
            } else if (menuHref.length >= currentPage.length) {
                if (menuHref.lastIndexOf(currentPage) == menuHref.length - currentPage.length) {
                    listItems.eq(i).addClass('selected-item');
                } else {
                    listItems.eq(i).removeClass('selected-item');
                }
            } else {
                if (currentPage.lastIndexOf(menuHref) == currentPage.length - menuHref.length) {
                    listItems.eq(i).addClass('selected-item');
                } else {
                    listItems.eq(i).removeClass('selected-item');
                }
            }
        }
    }

    function hideMenu(e) {
        if (!isMenuShowed)
            return false;
        if (e) {
            e.preventDefault();
        }
        isMenuShowed = !isMenuShowed;
        menuContainer.toggleClass('is-active');
        menuContainer.parents('body').find('.content-wrapper').toggleClass('show-menu', menuContainer.hasClass('is-active'));
        $('body').removeClass('overflow-hidden');
        stickySidebar(false);
        return false;
    }
    return {
        init: function (container) {
            menuContainer = container;
            init();
        },

        selectMenu: selectMenu,

        hideMenu: hideMenu,

        getContainer: function () {
            return menuContainer;
        }
    };
});
