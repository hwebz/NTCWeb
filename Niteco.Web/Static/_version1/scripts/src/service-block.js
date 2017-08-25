define(['jquery', 'underscore', 'base/modules/animate'], function ($, _, animate) {
    'use strict';

    var header = $('#whatWeDoContainer .block-header-contain');
    var listCertifications = $('#certificateContainer .certificate-item');
    var listServices = $('#serviceContainer .service-item');

    return {
        init: function (scrollToTop) {
            $('#whatWeDoContainer .panel-logo').on('click', function () {
                if (scrollToTop)
                    scrollToTop();
            });
        },
        
        rollbackAnimation: function () {
            animate(header, 'stop', true);
            animate(listCertifications.eq(0), 'stop', true);
            animate(listCertifications.eq(1), 'stop', true);
            animate(listCertifications.eq(2), 'stop', true);
            animate(listServices.eq(0), 'stop', true);
            animate(listServices.eq(1), 'stop', true);
            animate(listServices.eq(2), 'stop', true);
            animate(listServices.eq(3), 'stop', true);
            animate(listServices.eq(4), 'stop', true);
            
            header.css('opacity', '0');
            listCertifications.css('opacity', '0');
            listServices.css('opacity', '0');
        },
        
        playAnimation: function () {
            var durationHeader = 250;
            var durationCertificate = 250;
            var durationService = 250;
            $.when(animate(header, { opacity: 1 }, { duration: durationHeader }))
                .then(function () {
                    return animate(listCertifications.eq(1), { opacity: 1 }, { duration: durationCertificate });
                }).then(function () {
                    return $.when(animate(listCertifications.eq(0), 'transition.slideLeftBigIn', { duration: durationCertificate }),
                                animate(listCertifications.eq(2), 'transition.slideRightBigIn', { duration: durationCertificate }));
                }).then(function () {
                    return animate(listServices.eq(0), 'transition.slideUpIn', { duration: durationService });
                }).then(function () {
                    return animate(listServices.eq(1), 'transition.slideUpIn', { duration: durationService });
                }).then(function () {
                    return animate(listServices.eq(2), 'transition.slideUpIn', { duration: durationService });
                }).then(function () {
                    return animate(listServices.eq(3), 'transition.slideUpIn', { duration: durationService });
                }).then(function () {
                    return animate(listServices.eq(4), 'transition.slideUpIn', { duration: durationService });
                });
        }
    };
});