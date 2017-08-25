define(['jquery', 'underscore'], function ($, _) {

    'use strict';

    var deferred = $.Deferred(),
		stateEvents = {
		    '-1': 'unstarted',
		    '0': 'ended',
		    '1': 'playing',
		    '2': 'paused',
		    '3': 'buffering',
		    '5': 'cued'
		};

    window.onYouTubeIframeAPIReady = function () {
        deferred.resolve({
            create: createPlayer
        });
    };

    function init() {

        var script = document.createElement('script'),
			firstScriptTag = document.getElementsByTagName('script')[0];

        script.src = "https://www.youtube.com/iframe_api";

        firstScriptTag.parentNode.insertBefore(script, firstScriptTag);

    }

    function createPlayer(el, specs) {

        var deferred = $.Deferred(),
			options = _.extend(specs, {
			    playerVars: {
			        controls: 1,
			        autoplay: 1,
			        rel: 0,
			        showinfo: 0,
			        modestbranding: 1,
			        wmode: 'opaque',
                    fs: '1'
			    },
			    events: {
			        onReady: function () {
			            deferred.resolve(createApi(player));
			        },

			        onError: function () {
			            deferred.reject();
			        },

			        onStateChange: function (evt) {
			        }
			    }
			});

        var player = new YT.Player(el, options);

        return deferred.promise();
    }

    function createApi(player) {

        return {
            base: player,
            destroy: _.bind(player.destroy, player),
            play: _.bind(player.playVideo, player),
            pause: _.bind(player.pauseVideo, player),
            seek: _.bind(player.seekTo, player),
            mute: function () {
                player.mute();
            },
            unmute: function () {
                player.unMute();
            },
            setVolume: function (vol) {
                player.setVolume(vol);
            },
            getMuted: _.bind(player.isMuted, player),
            getVolume: _.bind(player.getVolume, player),
            getTime: _.bind(player.getCurrentTime, player),
            getProgress: function () {
                return player.getCurrentTime() / player.getDuration();
            },
            getDuration: _.bind(player.getDuration, player),
            getVideoUrl: _.bind(player.getVideoUrl, player)
        };
    }

    init();

    return deferred.promise();
});
