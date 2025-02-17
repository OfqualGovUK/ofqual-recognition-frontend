// sets the analytics (matomo) cookie to expire in 365 days
function setMatomoCookie(cvalue) {
    const d = new Date();
    const days = 365;
    d.setTime(d.getTime() + (days * 24 * 60 * 60 * 1000));
    let expires = "expires=" + d.toUTCString();
    document.cookie = "cookie_policy=" + cvalue + ";" + expires + ";path=/";
}

// checks if the cookie_policy is set and hides/shows the banner
function checkMatomoCookie() {
    let analytics = getCookie("cookie_policy");
    let banner = document.getElementById("analyticsBanner");

    if (analytics == "enabled" || analytics == "disabled") {
        banner.style.display = "none";
    } else {
        banner.style.display = "block";
    }
}

// sets the analytics (matomo) cookie to enabled
function acceptAnalytics() {
    setMatomoCookie("enabled");

    let elem = document.getElementById("acceptedCookies")
    elem.removeAttribute("hidden");

    let contentElem = document.getElementById("analyticsContent")
    contentElem.setAttribute("hidden", "");
}

// sets the analytics (matomo) cookie to disabled
function rejectAnalytics() {
    setMatomoCookie("disabled");

    let elem = document.getElementById("rejectedCookies")
    elem.removeAttribute("hidden");

    let contentElem = document.getElementById("analyticsContent")
    contentElem.setAttribute("hidden", "");
}

// checks if the cookie_policy is set and enabled
// returns false if cookie_policy is not set and not enabled
function analyticsEnabled() {
    let analytics = getCookie("cookie_policy");

    return analytics == "enabled";
}

// hides the analytics banner
function hideAnalytics() {
    let elem = document.getElementById("analyticsBanner")
    elem.style.display = "none";
}

// helper function to get any cookie
function getCookie(cName) {
    let name = cName + "=";
    let decodedCookie = decodeURIComponent(document.cookie);
    let ca = decodedCookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];

        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }

        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }

    return "";
}