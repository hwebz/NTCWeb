define(['vendor/handlebars'], function (Handlebars) {

    'use strict';

    Handlebars.registerHelper('ifNewRow', function (index, options) {
        if (index % 3 == 0) {
            return options.fn(this);
        } else {
            return options.inverse(this);
        }

    });
    
    Handlebars.registerHelper('ifEndRow', function (index, options) {
        if (index % 3 == 2) {
            return options.fn(this);
        } else {
            return options.inverse(this);
        }

    });

    return {};
});
