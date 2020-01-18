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
    }
    if (e.target.id == "loading-bar") {
        // Select the node that will be observed for mutations
        const targetNode = document.getElementById('loading-bar').firstChild;

        // Options for the observer (which mutations to observe)
        const config = { attributes: true, childList: true, subtree: true };
        // Callback function to execute when mutations are observed
        const callback = function (mutationsList, observer) {
            // Use traditional 'for loops' for IE 11
            for (let mutation of mutationsList) {
                if (mutation.type === 'attributes') {
                    var NewPercent = parseFloat(mutation.target.style.width) / 100;
                    if (logo == null) continue;
                    $('#LogoSVG').find('path').each(function (i, elem) {
                        $(this).animate({ 'stroke-dashoffset': elem.getTotalLength() * (1 - NewPercent) }, 100);
                    });
                }
            }
        };

        // Create an observer instance linked to the callback function
        const observer = new MutationObserver(callback);

        // Start observing the target node for configured mutations
        observer.observe(targetNode, config);
    }
});