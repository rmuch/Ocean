open Ocean
open Ocean.Core

let mainHandler req =
    RespondWith.str """<h1>Ocean Showcase</h1>"""

let routes =
    [ Match.path "/", mainHandler ]

[<EntryPoint>]
let main argv =
    FrameworkWebServer.serve "http://*:8080/" routes
    0
