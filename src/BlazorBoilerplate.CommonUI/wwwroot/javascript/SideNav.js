window.OpenNav = function () {
    document.getElementById("SideNav").style.right = "0";
    document.getElementById("NavButton").classList.remove("fa-bars");
    document.getElementById("NavButton").classList.add("fa-times");
}

window.CloseNav = function () {
    document.getElementById("SideNav").style.right = "100vw";
    document.getElementById("NavButton").classList.remove("fa-times");
    document.getElementById("NavButton").classList.add("fa-bars");
}