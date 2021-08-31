const spc = {
    matchMedia: function (args) {
        var mm = window.matchMedia(args);
        return { Matches: mm.matches, Media: mm.media };
    },
    setHtmlAttr: function (attr, value) {
        document.querySelector('html').setAttribute(attr, value);
    },
    setAttr: function (selector, attr, value) {
        document.querySelector(selector).setAttribute(attr, value);
    }
};

export { spc };
