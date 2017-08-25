define(['jquery', 'underscore', 'base/modules/animate', 'vendor/mobile-detect', 'templates/parallax_paging', 'templates/arrow_down'], function ($, _, animate, mobileDetect, parallaxPagingTmp, arrowDown) {
    'use strict';
    
    var enableScroll = true;
    var windowHeight = 700;
    var duration = 250;
    var numberPanel = 1;
    var currentPanel = 1;
    var listPanel = $('.parallax-panel');
    var body = $('body');
    var paging = null;
    var arrowDownBt = null;
    var sizeForStopParallax = 768;
    var isStopParallax = false;
    var callbackWhenStart = null;
    var callbackWhenEnd = null;

    function createParallaxScroll() {
        windowHeight = $(window).height();
        numberPanel = listPanel.length;
        if (!numberPanel || numberPanel <= 1)
            return;

        registerEvents();
        
        scrollToPanel(currentPanel, function() {
            createArrowDownAtTop();
            createParallaxPaging();
        });
    }
    
    function stopParallaxScroll() {
        body.removeClass('overflow-hidden');
        body.unbind('DOMMouseScroll mousewheel');
        if (arrowDownBt)
            arrowDownBt.css('display', 'none');
        if (paging)
            paging.removeClass('active');

        isStopParallax = true;
    }
    
    function enableParallaxScroll() {
        body.addClass('overflow-hidden');
        registerEvents();
        
        if (arrowDownBt)
            arrowDownBt.css('display', 'block');
        if (paging)
            paging.addClass('active');
        
        isStopParallax = false;
    }

    function registerEvents() {
        body.on('DOMMouseScroll mousewheel', function (e) {
            if (!enableScroll)
                return false;

            if (e.originalEvent.detail > 0 || e.originalEvent.wheelDelta < 0) {
                scrollDown();
            }
            else {
                scrollUp();
            }

            return false;
        });

        var scrollToElement = _.debounce(function () {
            windowHeight = $(window).height();
            scrollToPanelWithoutAnimate(currentPanel);
        }, 100);

        $(window).unbind('resize');
        $(window).on('resize', function () {
            var windowWidth = $(window).width();
            if (windowWidth < sizeForStopParallax) {
                stopParallaxScroll();
                return;
            }
            if (isStopParallax) {
                enableParallaxScroll();
            }

            scrollToElement();
        });
        
        $(document).on('keypress', function (e) {
            if (isStopParallax)
                return true;
            switch (e.keyCode) {
                case 38: // up key
                    scrollUp();
                    return false;
                case 40: // dow key
                    scrollDown();
                    return false;
            }

            return true;
        });
    }

    function createParallaxPaging() {
        var arrNumberPanel = new Array();
        for (var i = 0; i < numberPanel; i++)
            arrNumberPanel.push(1);
        
        paging = $(parallaxPagingTmp({ list: arrNumberPanel }));
        body.append(paging);

        paging.find('li').eq(0).addClass('active');

        registerEventPaging();
    }
    
    function createArrowDownAtTop() {
        arrowDownBt = $(arrowDown());
        listPanel.eq(0).append(arrowDownBt);
        
        var scrollToElement = _.debounce(function (targetPanel) {
            scrollToPanel(targetPanel);
        }, 100);

        arrowDownBt.on('click', function() {
            scrollToElement(2);
        });
    }
    
    function registerEventPaging() {
        var scrollToElement = _.debounce(function (targetPanel) {
            scrollToPanel(targetPanel);
        }, 100);

        var listPages = paging.find('li');
        for (var i = 0; i < listPages.length; i++) {
            (
                function(i) {
                    listPages.eq(i).on('click', function () {
                        scrollToElement(i + 1);
                    });
                }
            )(i);
        }
    }
    
    function setActivePage() {
        if (paging) {
            paging.find('li').removeClass('active');
            paging.find('li').eq(currentPanel - 1).addClass('active');
            
            if (currentPanel <= 1) {
                paging.removeClass('active');
            } else {
                paging.addClass('active');
            }
        }
    }

    function scrollUp() {
        if (currentPanel > 1) {
            scrollToPanel(currentPanel - 1);
        }
    }

    function scrollDown() {
        if (currentPanel < numberPanel) {
            scrollToPanel(currentPanel + 1);
        }
    }
    
    function scrollToPanelWithoutAnimate(panelIndex) {
        enableScroll = false;

        body.velocity('scroll', {
            duration: 0, offset: listPanel.eq(panelIndex - 1).offset().top, easing: 'ease-in-out',
            complete: function () {
                enableScroll = true;
                currentPanel = panelIndex;
                setActivePage();
            }
        });
    }

    function scrollToPanel(panelIndex, callback) {
        enableScroll = false;
        if (callbackWhenStart)
            callbackWhenStart(listPanel.eq(currentPanel - 1), currentPanel);
        body.velocity('scroll', {
            duration: duration + duration / 3 * Math.abs(currentPanel - panelIndex), offset: listPanel.eq(panelIndex-1).offset().top, easing: 'ease-in-out',
            complete: function () {
                enableScroll = true;
                
                currentPanel = panelIndex;
                setActivePage();

                if (callback)
                    callback();
                if (callbackWhenEnd)
                    callbackWhenEnd(listPanel.eq(currentPanel-1), currentPanel);
            }
        });
    }

    return {
        init: function (callbackStartScroll, callbackEndScroll) {
            callbackWhenStart = callbackStartScroll;
            callbackWhenEnd = callbackEndScroll;
            
            var paralaxScroll = body.attr('data-scroll');
            var md = new mobileDetect(window.navigator.userAgent);
            if (!!paralaxScroll && paralaxScroll.toLowerCase() == "true" && !md.mobile()) {
                createParallaxScroll();
            } else {
                body.css('overflow', 'auto');
            }
        },
        scrollToTop: function() {
            scrollToPanel(1);
        }
    };
});