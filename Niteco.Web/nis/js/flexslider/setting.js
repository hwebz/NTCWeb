(function($) {
	'use strict';
      $('.flexslider').flexslider({
        animation: "slide",
		directionNav: true,
		controlNav: true,
		slideshow: false, 
      });
	  
      $('.imac-device').flexslider({
        animation: "slide",
		directionNav: false,
		controlNav: false,
		slideshow: false, 
		pausePlay: true, 
		mousewheel: true,
      });
})(jQuery);