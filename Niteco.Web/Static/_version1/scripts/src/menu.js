define(['jquery', 'underscore', 'base/modules/animate'], function ($, _, animate) {
    'use strict';

    var isShowMenu = false;

    var menuDown = $("#menu-down");

    var menuContent = $('#menuContent');

    function showMenu() {
        $("#btnMenu").toggleClass("is-active");
        $.when(animate(menuDown, {
            bottom: "0"
        }, { duration: 500, easing: 'ease-in-out', delay: 0 })).
    then(function () {
        return animate(menuContent, 'transition.slideUpIn', {
            duration: 250,
            easing: 'ease-in-out',
            delay: 0
        });
    }).then(function () {
        isShowMenu = true;
    });
    }

    function hideMenu() {
        $("#btnMenu").toggleClass("is-active");
        $.when(animate(menuContent, 'transition.slideDownOut', { duration: 250, easing: 'ease-in-out', delay: 0 })).then(function () {
            return animate(menuDown, { bottom: "100%" }, {
                duration: 500,
                easing: 'ease-in-out',
                delay: 0
            });
        }).then(function () {
            isShowMenu = false;
        });
    }

    return {
        init: function () {
            $("#btnMenu").on('click', function () {
                if (!isShowMenu) {
                    showMenu();
                } else {
                    hideMenu();
                }
            });
            $(".tab a").hover(function () {
                if (!$(this).hasClass('tab-active')) {
                    $(".tab").toggleClass("tab-hover");
                }

            });
            $("#menu-content li").on('click', 'a', function () {
                $("a").removeClass("tab-active");
                $(this).addClass("tab-active");
            });
            $(document).on('keyup', function (e) {
                if (!isShowMenu)
                    return;
                if (e.keyCode === 27) {
                    hideMenu();
                }
            });
        }
    };
});