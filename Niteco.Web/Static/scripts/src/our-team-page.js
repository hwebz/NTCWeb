define(['jquery', 'underscore', 'base/list-our-team-expand', 'base/page-scroll-animation'], function ($, _, listOurTeam, pageScrollAnimation) {
    function init(container) {
        pageScrollAnimation.init(container);
    }

    return {
        init: function (container) {
            init(container);
            listOurTeam.init(container);
        }
    };
});