define(['jquery', 'underscore', 'base/modules/video_youtube_module'], function ($, _, videoModule) {

    'use strict';

    function init(container) {
        var backboneView = backboneInit();
        return new backboneView({
            el: container
        });

    }

    function backboneInit() {
        return function BackboneView(options) {
            this.enablePlayVideo = true;
            this.$el = options.el;
            this.options = {
                playerClass: 'player-video'
            };

            this.initVideo = function () {
                this.$el.find('a[data-youtube-button]').on('click', _.bind(this.startVideo,this));
                this.videoId = this.getYoutubeId(this.$el.find('a').attr('href'));
            };


            this.startVideo = function (evt) {
                evt.preventDefault();

                if (!this.enablePlayVideo)
                    return;
                this.enablePlayVideo = false;

                videoModule.then(_.bind(this.loadVideo, this));
            };

            this.loadVideo = function (api) {
                this.$playerContainer = $('<div class="' + this.options.playerClass + '"></div>');
                this.$el.append(this.$playerContainer);

                return api.create(this.$playerContainer.get(0), {
                    height: '100%',
                    width: '100%',
                    videoId: this.videoId
                }).then(_.bind(this.ready, this));
            };

            this.ready = function (player) {
                this.player = player;
            };

            this.removeVideo = function () {
                if (this.player) {
                    this.player.destroy();
                }
                this.$playerContainer.remove();
            },

            this.getYoutubeId = function (url) {
                if (!url && url.length === 0)
                    return url;

                var id = '';

                url = url.replace(/(>|<)/gi, '').split(/(vi\/|v=|\/v\/|youtu\.be\/|\/embed\/)/);

                if (url[2] !== undefined) {
                    id = url[2].split(/[^0-9a-z_\-]/i);
                    id = id[0];
                }
                else {
                    id = url;
                }

                return id;
            }


            // Auto start
            this.initVideo();
        }
    }


    return {
        init: function (container) {
            return init(container);
        }
    };
});
