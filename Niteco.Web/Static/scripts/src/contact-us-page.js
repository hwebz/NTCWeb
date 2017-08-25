define(['base/page-scroll-animation'], function (pageScrollAnimation) {

    function initScrollAnimation(container) {
        pageScrollAnimation.init(container);

        if (window.google && window.google.maps && window.google.maps.Map) {
            initMap();
        }
    }
    
    return {
        init: function (container) {
            initScrollAnimation(container);
        }
    };
});