open Ocean
open Ocean.Core

let mainHandler req =
    RespondWith.str """<p>Hello from Ocean!</p>"""

let routes =
    [ Match.path "/", mainHandler ]

[<EntryPoint>]
let main argv =
    FrameworkWebServer.serve "http://*:8080/" routes
    0
