/// Contains static assets used by Ocean default responses and other parts of
/// the framework.
[<RequireQualifiedAccess>]
module internal Ocean.Resources

let internalPageStylesheet =
    """body {
  font-family: 'Segoe UI', sans-serif;
  font-weight: 400;
  max-width: 720px;
  margin: 0 auto;
}
h1,
h2 {
  font-weight: 400;
}
.ocean-powered-by {
  font-size: small;
}
code {
  background-color: rgba(0, 238, 114, 0.07);
  padding: 8px;
}
code.ocean-stack-trace,
code.ocean-code-block {
  display: block;
  overflow-x: auto;
}
pre {
  margin: 0;
}
code,
pre {
  font-family: 'Consolas', monospace;
  font-size: small;
}
a {
  color: rgba(0, 164, 78, 0.75);
  -moz-transition: color 0.33s ease-in-out;
  -o-transition: color 0.33s ease-in-out;
  -webkit-transition: color 0.33s ease-in-out;
  transition: color 0.33s ease-in-out;
}
a:hover {
  color: #019045;
  -moz-transition: color 0.33s ease-in-out;
  -o-transition: color 0.33s ease-in-out;
  -webkit-transition: color 0.33s ease-in-out;
  transition: color 0.33s ease-in-out;
}
::selection {
  background-color: rgba(0, 238, 114, 0.33);
}
::-moz-selection {
  background-color: rgba(0, 238, 114, 0.33);
}"""

let poweredByLine =
    """<p class="ocean-powered-by">Powered by <a href="#">Ocean</a></p>"""

let internalPageStart =
    """<!doctype html><html><head><style>""" + internalPageStylesheet + """</style></head><body>"""

let internalPageEnd =
    poweredByLine + """</body></html>"""
