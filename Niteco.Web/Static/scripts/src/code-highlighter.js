define(['jquery', 'base/modules/codemirror'], function () {
    'use strict';

    var editor = null;

    function initialize() {
        editor = CodeMirror.fromTextArea(document.getElementById("code"), {
            lineNumbers: false,
            theme: "night",
            mode: "text/x-java"
        });

        $("#code-expand").on('click', function (e) {
            e.preventDefault();

            toggleFullscreenEditing($(this));

            return false;
        });
    }

    function setThumbnailBackground(thumbnailClasses) {
        var thumbnailWrapper = $(thumbnailClasses);

        thumbnailWrapper.each(function () {
            var imageSrc = $(this).find("img");
            $(this).css('background-image', 'url(' + imageSrc.attr("src") + ')');
            imageSrc.hide();
        });
    }

    function toggleFullscreenEditing(ele) {
        var editorDiv = $('#code-wrapper');
        var editorDivEle = $('.CodeMirror-scroll');
        if (!editorDiv.hasClass('fullscreen')) {
            ele.html('Collapse <span class="fa fa-compress"></span>');
            toggleFullscreenEditing.beforeFullscreen = { height: editorDiv.height(), width: editorDiv.width() }
            editorDiv.addClass('fullscreen');
            editorDiv.height('100%');
            editorDiv.width('100%');
            editorDivEle.addClass('fullscreen2');
            editorDivEle.height("96vh");
            editor.refresh();
            $("body").css("overflow", "hidden");
            $(".scroll-menu").removeAttr("style");
        }
        else {
            ele.html('Expand <span class="fa fa-expand"></span>');
            editorDiv.removeClass('fullscreen');
            editorDiv.height(toggleFullscreenEditing.beforeFullscreen.height);
            editorDiv.width(toggleFullscreenEditing.beforeFullscreen.width);
            editorDivEle.removeClass('fullscreen2');
            editorDivEle.height("auto");
            editor.refresh();
            $("body").removeAttr("style");
        }
    }

    return {
        init: function (container) {
            setThumbnailBackground(".blog-list-articles__article-thumbnail");
            return new initialize();
        }
    }
});