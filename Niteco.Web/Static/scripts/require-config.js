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
			videojs: 'http://vjs.zencdn.net/4.12/video',
			underscore: 'lib/bower/underscore',
			vendor: 'lib/bower',
			templates: 'templates'
		},
		map: {
		    '*': {
		        'handlebars': 'vendor/handlebars',
		        'velocity': 'vendor/velocity',
		        'async': 'vendor/requirejs-plugins/async',
		        'selectize': 'lib/selectize',
		        'sifter': 'lib/sifter',
		        'microplugin': 'lib/microplugin'
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
		    },
		    'lib/selectFx': {
		        exports: 'SelectFx',
		        deps: [
		            'lib/classie'
		        ]
		    },
		    'lib/classie': {
		        exports: 'classie'
		    },
		    'vendor/mobile-detect': {
		        exports: 'MobileDetect'
		    },
		    'lib/jquery.prettyPhoto': {
		        deps: [
		            'jquery'
		        ]
		    },
		    'lib/picturefill': {
		    }
		}
	});
	return requirejs;
}));
