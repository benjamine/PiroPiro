

$(function () {
    // show back to top only when scrolled down
    var w = $(window), backToTop = $('.back-to-top').hide();
    if (backToTop.length) {
        var backToTopVisible = false;
        var updateBackToTop = function () {
            if (w.scrollTop() > 0) {
                if (!backToTopVisible) {
                    $('.back-to-top').stop(true).fadeIn(function () {
                        backToTopVisible = true;
                    });
                }
            } else {
                if (backToTopVisible) {
                    $('.back-to-top').stop(true).fadeOut(function () {
                        backToTopVisible = false;
                    });
                }
            }
        };
        w.scroll(updateBackToTop);
        updateBackToTop();
    }

})