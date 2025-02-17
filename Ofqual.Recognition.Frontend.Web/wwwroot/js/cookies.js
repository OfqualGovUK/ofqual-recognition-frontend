if (analyticsEnabled()) {
    document.getElementById("cookies-analytics-yes").setAttribute("checked", "");
    document.getElementById("cookies-analytics-no").removeAttribute("checked");
} else {
    document.getElementById("cookies-analytics-yes").removeAttribute("checked");
    document.getElementById("cookies-analytics-no").setAttribute("checked", "");
}

function submitCookieForm() {
    var val = document.querySelector('input[name="cookies-analytics"]:checked').value

    if (val == "yes") {
        setMatomoCookie("enabled");
    } else {
        setMatomoCookie("disabled");
    }

    document.getElementById("cookiesFormSubmitted").removeAttribute("hidden");
    window.scrollTo(0, 0);
}

function goBackWithRefresh(event) {
    if ('referrer' in document) {
        window.location = document.referrer;
    } else {
        window.history.back();
    }
}