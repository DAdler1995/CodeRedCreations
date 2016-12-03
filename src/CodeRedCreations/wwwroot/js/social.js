// TWITTER START \\
window.twttr = (function (d, s, id) {
    var js, fjs = d.getElementsByTagName(s)[0],
      t = window.twttr || {};
    if (d.getElementById(id)) return t;
    js = d.createElement(s);
    js.id = id;
    js.src = "https://platform.twitter.com/widgets.js";
    fjs.parentNode.insertBefore(js, fjs);

    t._e = [];
    t.ready = function (f) {
        t._e.push(f);
    };

    return t;
}(document, "script", "twitter-wjs"));

function tweet(product, url) {
    window.open('https://twitter.com/intent/tweet?url=' + encodeURIComponent(url) + '&display_url=CodeRedPerformance&text=' + encodeURIComponent('I really want the ' + product + ' on Code Red Performance.'));
}
// TWITTER END \\

// FACEBOOK START \\

window.fbAsyncInit = function () {
    FB.init({
        appId: '980364412074030',
        xfbml: true,
        version: 'v2.8'
    });
    FB.AppEvents.logPageView();
};

(function (d, s, id) {
    var js, fjs = d.getElementsByTagName(s)[0];
    if (d.getElementById(id)) { return; }
    js = d.createElement(s); js.id = id;
    js.src = "//connect.facebook.net/en_US/sdk.js";
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk'));

function share(product, url) {
    FB.ui(
        {
            method: 'share',
            href: url
        });
}
// FACEBOOK END \\