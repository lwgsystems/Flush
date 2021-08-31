﻿// Update the toggle button based on current color scheme
function updateDarkToggleButton() {
    $mode = $("#css").attr("data-color-scheme");
    $dark = (window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches);
    if (typeof $mode !== 'undefined') {
        $dark = $mode === 'dark';
    }
    $("#css-toggle-btn").prop("checked", $dark);
}

// and every time it changes
if (window.matchMedia) window.matchMedia("(prefers-color-scheme: dark)").addListener(updateDarkToggleButton);

// Color Scheme toggle botton

// function to initialise the css
function init_color_scheme_css($id, $mode) {
    if ($("#" + $id)) $("#" + $id).remove();  // remove exitsing id
    $("#" + $id + "-" + $mode).attr({
        "data-href-light": $("#" + $id + "-light").attr("href"),  // store the light CSS url
        "data-href-dark": $("#" + $id + "-dark").attr("href"), // store the dark CSS url
        "data-color-scheme": $mode,  // store the mode, so that we don't re-initalise
        "media": "all",  // drop the media filter
        "id": $id  // rename the id (drop the `-{mode}` bit)
    });
    $other = ($mode == 'dark') ? 'light' : 'dark';
    $("#" + $id + "-" + $other).remove();
}

// function to toggle the CSS
function toggle_color_scheme_css($id, $mode) {
    // grab the new mode css href
    $href = $("#" + $id).data("href-" + $mode);  // use `.data()` here, leverage the cache
    // set the CSS to the mode preference.
    $("#" + $id).attr({
        "href": $href,
        "data-color-scheme": $mode,
    });
}

// toggle button click code
function darkModeToggleClicked() {
    // get current mode
    // don't use `.data("color-scheme")`, it doesn't refresh
    $mode = $("#css").attr("data-color-scheme");
    // test if this is a first time click event, if so initialise the code
    if (typeof $mode === 'undefined') {
        // not defined yet - set pref. & ask the browser if alt. is active
        $mode = 'light';
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) $mode = 'dark';
        init_color_scheme_css("css", $mode);
        // `init_color_scheme_css()` any other CSS
    }
    // by here we have the current mode, so swap it
    $new_mode = ($mode == 'dark') ? 'light' : 'dark';
    toggle_color_scheme_css("css", $new_mode);
    // `toggle_color_scheme_css()` any other CSS
}
