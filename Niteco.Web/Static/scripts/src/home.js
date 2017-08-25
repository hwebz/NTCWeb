define(['jquery', 'underscore', 'videojs', 'base/request-page', 'base/top-navigator', 'vendor/mobile-detect'], function ($, _, videojs, requestPage, topNavigator, MobileDetect) {

    'use strict';

    var wrapper = null;
    var videoSize = { width: 0, height: 0 };
    var videoWrapper = null;
    var videoJs = null;
    var videoObj = null;
    var isInitLink = false;
    
    var md = new MobileDetect(window.navigator.userAgent);

    function initVideoJs() {
        if (videoObj != null && videoObj.length > 0) {
            videojs(videoObj.get(0), {
                autoplay: true,
                loop: true
            }, function () {
                this.on('loadedmetadata', function () {
                    videoObj = videoWrapper.find('video');
                    videoSize.width = videoObj.get(0).videoWidth;
                    videoSize.height = videoObj.get(0).videoHeight;
                    videoJs = videoObj.parent();
                    changeVideoOnResize();
                    registerEvent();
                });
            });
        }
        
        registerEventForLinks();
    }
    
    function changeVideoOnResize() {
        if (videoObj != null && videoObj.length > 0) {
            var changeStyleVideo = _.debounce(function () {
                var containerHeight = videoWrapper.height();
                var containerWidth = videoWrapper.width();

                videoJs.css({
                    width: containerWidth + 'px',
                    height: containerHeight + 'px'
                });

                if (containerWidth / containerHeight > videoSize.width / videoSize.height) {
                    videoObj.css({
                        width: containerWidth + 'px',
                        height: 'auto',
                        opacity: 1
                    });
                } else {
                    videoObj.css({
                        width: 'auto',
                        height: containerHeight + 'px',
                        opacity: 1
                    });
                }
            }, 50);

            changeStyleVideo();
        }
    }
    
    function updateSelectMenu(href) {
        topNavigator.selectMenu(href);
    }
    
    function registerEvent() {
        $(window).on('resize', function () {
            changeVideoOnResize();
        });
    }
    
    function registerEventForLinks() {
        if (isInitLink)
            return;

        isInitLink = true;
        requestPage.registRequestByContainer(wrapper.find('.home-column-left'), updateSelectMenu);
        requestPage.registRequestByContainer(wrapper.find('.home-column-right'), updateSelectMenu);
    }

    return {        
        init: function (container) {
            wrapper = container;
            videoWrapper = container.find('.home-video-wrapper');
            videoObj = videoWrapper.find('video');
            initVideoJs();
        }
    };
});
