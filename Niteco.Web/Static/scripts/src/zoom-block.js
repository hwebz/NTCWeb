define(['jquery', 'underscore', 'base/modules/animate', 'vendor/mobile-detect', 'lib/picturefill'], function ($, _, animate, MobileDetect) {
    'use strict';


    var $items, $overlay, $contentLoading, $close,
        // transition end event name

        // window and body elements
        $window = $(window), bufferHeight = 0,
        // transitions support

        // current item's index
        current = -1;

    var md = new MobileDetect(window.navigator.userAgent);
    function initEvents() {


        $items = $('.project-item');

        if ($(".project-modal").length == 0) {
            var projectModal = getHTMLModal();
            $(".content-container").append(projectModal);

        }

        $overlay = $(".project-modal");
        $close = $overlay.find('span.rb-close');
        $contentLoading = $overlay.find('.project-loading-content');

     


        $items.each(function() {

            var $item = $(this);
            // on non IOS device will open modal dialog
            if (!navigator.userAgent.match(/(iPhone|iPod|iPad)/i) && !md.mobile()) {
                bufferHeight = 80;
                $item.find('a').on('click', function (e) {
                    e.preventDefault();
                    showModel($item);
                    return false;
                });
                $item.on('click', function () {
                    showModel($item);
                });
            } else {// on IOS device will redirect to new page
                $item.on('click', function () {
                    $item.find('a').trigger("click");
                });
            }
        });

        $close.on('click', function() {
            closeModalDialog();
        });        
       
       
    }

   

    function disableScroll() {
        $('body').addClass('overflow-hidden');
     
    }

    function enableScroll() {
            $('body').removeClass('overflow-hidden');
           
        }

        function showModel($item) {
            disableScroll();
         
            current = $item.index();

            $overlay.children(".project-preview").html($item.html());

            var layoutProp = getItemLayoutProp($item);
            $overlay.css({
                top: layoutProp.top,
                left: layoutProp.left,
                width: layoutProp.width,
                height: layoutProp.height,
                zIndex: 9999,
                pointerEvents: 'auto',
            });
            $overlay.show();
          
            $overlay.animate({ top: (0 + $window.scrollTop()), left: 0, right: 0, bottom: 0, width: '100%', height: ('100%') }, 500, function () {
                $overlay.css({ overflow: 'hidden' });
                $close.css({ opacity: 1 });
                bindEscapeKey();
                $.ajax({
                    url: $item.attr('data-url'),
                }).done(function(data) {                    
                    $contentLoading.html($(data).find(".section-case-study-detail").html()).promise().done(function () {
                        window.picturefill();
                        $overlay.focus();
                        //$(document).keydown(function (e) {
                        //    if (e.keyCode > 36 && e.keyCode < 41) {
                        //        $overlay
                        //    }
                        //});
                    });
                    //Work around for FF bug: FF always keeps page scroll position of element
                    if (navigator.userAgent.toLowerCase().indexOf('firefox') > -1) {
                        setTimeout(function () {
                            $overlay.scrollTop(0);
                        }, 10);
                    }
                    
                });
                $overlay.on('click', function() {
                    closeModalDialog();
                });

            });
        }

        

        function closeModalDialog() {
            enableScroll();

            if (current !== -1) {
                var layoutProp = getItemLayoutProp($items.eq(current));
            
                $close.css({ opacity: 0 });
                $overlay.animate({ top: layoutProp.top, left: layoutProp.left, width: layoutProp.width, height: layoutProp.height }, 500, function() {
                    $overlay.css({
                        zIndex: 0,
                        pointerEvents: 'none',
                    });
                    $overlay.hide();
                    $overlay.children(".project-preview").empty();
                    $contentLoading.html(getHTMLLoadingContent());
                });

            }
            current = -1;
        }

        function bindEscapeKey() {
            $("body").bind("keyup.myDialog", function(event) {
                if (event.which == 27) {
                    // TODO: close the dialog

                    closeModalDialog();

                    $("body").unbind("keyup.myDialog");
                }
            });
        }

        function getItemLayoutProp($item) {

            var scrollT = $window.scrollTop(),
                scrollL = $window.scrollLeft(),
                itemOffset = $item.offset();

            return {
                left: itemOffset.left,
                top: itemOffset.top,
                width: $item.outerWidth(),
                height: $item.outerHeight()
            };

        }


        function getHTMLModal() {
            var projectModal = "<div class=\"project-modal js-project-modal\">";
            //projectModal += "<span class=\"rb-close\">close</span>";
            projectModal += "        <div class=\"project-preview\">";
            projectModal += "       <\/div>";
            projectModal += "        <div class=\"project-loading-content ajax-content-container section-case-study-detail\">";
            projectModal += getHTMLLoadingContent();
            projectModal += "   <\/div>";
            return projectModal;

        }
    
        function getHTMLLoadingContent() {
            var projectModal = "           <div class=\"loading-container-2\">";
            projectModal += "               <div class=\"loading\"></div>";
            projectModal += "               <div id=\"loading-text\">loading</div>";
            projectModal += "           </div>";
            projectModal += "       <\/div>";
            return projectModal;
        }


        return {
            init: function() {
                initEvents();
            }
        };
    
});



