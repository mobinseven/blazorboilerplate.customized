/* TODO: Server Side Dummy Loader */
var logo = null;
$("body").bind("DOMNodeInserted", function (e) {
    if (e.target.id == "LogoSVG") {
        logo = document.getElementById("LogoSVG");
        var paths = logo.getElementsByTagName('path');
        for (var i = 0; i < paths.length; i++) {
            var length = paths[i].getTotalLength();

            // The start position of the drawing
            paths[i].style.strokeDasharray = length;

            // Hide the triangle by offsetting dash. Remove this line to show the triangle before scroll draw
            paths[i].style.strokeDashoffset = length;
        }
        $('#LogoSVG').find('path').each(function (i, elem) {
            $(this).animate({ 'stroke-dashoffset': 0 }, {
                duration: 4000,
                easing: 'linear'
            });
        });
    }
});