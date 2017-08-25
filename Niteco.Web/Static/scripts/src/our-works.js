define(['jquery', 'underscore', 'base/zoom-block', 'base/page-scroll-animation'], function ($, _, zoomBlock, pageScrollAnimation) {
    function init(container) {
        pageScrollAnimation.init(container);
    }
    return {
        init: function (container) {
            init(container);
            zoomBlock.init();   
        }
    };
});