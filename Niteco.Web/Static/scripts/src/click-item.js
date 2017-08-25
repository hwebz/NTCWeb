define(['jquery', 'underscore','base/top-navigator', 'base/request-page'], function ($, _, topNavigator, requestPage) {

    'use strict';
    
    function init(container) {
        requestPage.registRequestByContainer(container, updateSelectMenu);
    }
    
    function updateSelectMenu(href) {
        topNavigator.selectMenu(href);
    }

    return {
        init: init
    };
});
