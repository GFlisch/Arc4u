

window.Arc4u = {
    CreatePopupWindow: function (authority, feature) {

        const win = window.open("_content/Arc4u.Standard.OAuth2.Blazor/authenticate.html", "_blank", feature);

        win.onload = function () {

            win.location.href = authority;
        };
    }
};
