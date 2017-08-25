(function (root, factory) {
	'use strict';
	var requireJsShim = {
		cache: null,
		config: function (config) {
			if (config)
				this.cache = config;
			return this.cache;
		}
	};
	if (typeof define === 'function' && define.amd && typeof requirejs !== 'undefined') {
		factory(requirejs);
	} else if (typeof exports === 'object') {
		var config = factory(requireJsShim).config();
		module.exports = config;
	}
}(this, function (requirejs) {
	'use strict';
	requirejs.config({
		baseUrl: '/../js/',
		paths: {
			base: 'src',
			jquery: 'https://ajax.googleapis.com/ajax/libs/jquery/2.1.4/jquery.min',
			underscore: 'lib/bower/underscore',
			vendor: 'lib/bower',
			templates: 'templates',
			velocity: 'lib/bower/velocity',
			'velocity.ui': 'lib/bower/velocity.ui'
		},
		map: {
		    '*': {
		        'handlebars': 'vendor/handlebars'
		    }
		},
		shim: {
		    'vendor/handlebars': {
		        exports: 'Handlebars'
		    },
		    'velocity': {
		        deps: [
		            'jquery'
		        ]
		    },
		    'velocity.ui': {
		        deps: [
		            'velocity'
		        ]
		    },
		    'lib/owl.carousel': {
		        exports: 'jQuery.fn.owlCarousel',
		        deps: [
		            'jquery'
		        ]
		    }
		}
	});
	return requirejs;
}));
