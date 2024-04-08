function getCookie(cookieName) {
    var name = cookieName + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

document.addEventListener("DOMContentLoaded", () => {

    var cookieNotify = getCookie('Notify');
    if (cookieNotify !== "") {
        Toastify({
            text: cookieNotify,
            duration: 3000,
            close: true,
            offset: {
                x: 9,
                y: 57,
            },
        }).showToast();
    }
    var cookieError = getCookie('Error');
    if (cookieError !== "") {
        Toastify({
            text: cookieError,
            duration: 3000,
            close: true,
            offset: {
                x: 9,
                y: 57,
            },
        }).showToast();
    }
});


