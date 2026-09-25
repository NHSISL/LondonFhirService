// nhsuk-frontend 10 refuses to initialise its components - and nhsuk-react-components 6 throws while
// rendering them - unless <body> carries nhsuk-frontend-supported. Its own page template adds the
// class with an inline script; this does the same from a module instead, so it keeps working if
// the host ever sends a Content-Security-Policy that blocks inline script.
//
// No feature test is needed: a browser that runs this module supports ES modules, which is the
// very thing the template's `'noModule' in HTMLScriptElement.prototype` check was asking.
document.body.classList.add("js-enabled", "nhsuk-frontend-supported");

export {};
