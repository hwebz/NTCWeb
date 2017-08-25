define(['jquery', 'underscore'], function ($, _) {

    'use strict';

    $win = $(window);
    breakpoints = {        
        small: 480,
        medium: 640,
        large: 1024,
        xlarge: 1200,
        xxlarge: 1900
    };

    function init(container) {
        var coverBg = new ResponsiveCover(container);
        coverBg.changeImageCover();
        
        $win.on('resize', coverBg.changeImageCover);
        $win.on('orientationchange', coverBg.changeImageCover);
    }
    
    function ResponsiveCover(container) {
        
        this.changeImageCover = function() {
            var width = $win.width();
        }
    }

    return {
        init: init
    };
});
