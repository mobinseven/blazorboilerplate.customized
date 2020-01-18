var scroller = "container";
window.SaveScrollPosition = function () {
    var LastPage = localStorage.getItem("LastPage");
    var scrollPosition = document.getElementById(scroller).scrollTop;
    localStorage.setItem("scrollPosition_" + LastPage, scrollPosition.toString());
}
window.SaveCurrentPagePath = function () {
    var pathName = document.location.pathname;
    localStorage.setItem("LastPage", pathName);
}
window.LoadScrollPosition = function () {
    var pathName = document.location.pathname;
    var scroll = parseInt(localStorage.getItem("scrollPosition_" + pathName));
    if (!isNaN(scroll))
        document.getElementById(scroller).scrollTop = scroll;
}
window.ScrollToBottom = function () {
    var elem = document.getElementById(scroller);
    elem.scrollTop = elem.scrollHeight;
}