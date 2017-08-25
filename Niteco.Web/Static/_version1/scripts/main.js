define(['jquery', 'underscore',
    'base/parallax-scroll',
    'base/aboutUs-block',
    'base/service-block',
    'base/menu',
    'base/contact-block',
    'base/product-block',
    'base/testimonial-block',
    'base/logo-scroll'
],
    function ($, _, parallaxScroll, aboutUsBlock, serviceBlock, menu, contactBlock, productBlock, testimonialBlock, logoScroll) {
        'use strict';
        menu.init();
        logoScroll.init();
        aboutUsBlock.init(parallaxScroll.scrollToTop);
        serviceBlock.init(parallaxScroll.scrollToTop);
        contactBlock.init(parallaxScroll.scrollToTop);
        productBlock.init(parallaxScroll.scrollToTop);
        testimonialBlock.init(parallaxScroll.scrollToTop);

        var mappingPanelHome = {            
            introduction: 'introduction',
            about: 'about-us',
            service: 'what-we-do',
            product: 'product',
            testimonial: 'testimonial',
            contact: 'contact',
            
            getPanelObj: function(panelData){
                switch(panelData) {
                    case this.introduction:
                        return null;
                    case this.about:
                        return aboutUsBlock;
                    case this.service:
                        return serviceBlock;
                    case this.product:
                        return productBlock;
                    case this.testimonial:
                        return testimonialBlock;
                    case this.contact:
                        return contactBlock;
                }
            }
        };

        var previousPanel = null;
        var currentPanel = null;

        var callbackScrollStart = function (panelObj, panelIndex) {
            logoScroll.callbackBeforeScroll(panelIndex);
        };

        var callbackScrollEnd = function (panelObj, panelIndex) {
            logoScroll.callbackAfterScroll(panelIndex);
            
            var panelData = panelObj.attr('data-panel');

            previousPanel = currentPanel;
            currentPanel = mappingPanelHome.getPanelObj(panelData);
            
            if (previousPanel && previousPanel.rollbackAnimation) {
                previousPanel.rollbackAnimation();
            }
            if (currentPanel && currentPanel.playAnimation) {
                currentPanel.playAnimation();
            }
        };
        

        parallaxScroll.init(callbackScrollStart, callbackScrollEnd);
});