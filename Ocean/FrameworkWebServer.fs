﻿/// Simple web server implementation using the .NET Framework's
/// System.Net.HttpListener class.
///
/// When running on Windows, ensure that the URL ACL is properly configured
/// for your listen interface. For more information, consult the netsh
/// documentation.
module Ocean.FrameworkWebServer

open Ocean.Core

open System.IO
open System.Net

/// Translate a System.Net.HttpListenerContext to a Ocean.Request, ready to
/// be handled by the framework.
let private translateRequest (ctx : HttpListenerContext) : Request =
    let url = ctx.Request.Url
    let mutable headers = []
    for i = 0 to ctx.Request.Headers.Count - 1 do
        headers <- headers @ [ (ctx.Request.Headers.GetKey(i), ctx.Request.Headers.GetValues(i) |> Array.toList) ]
    { Request.empty with Url = url
                         Headers = headers
                         BodyReader = fun _ -> new StreamReader(ctx.Request.InputStream)
                         RemoteEndPoint = ctx.Request.RemoteEndPoint }

/// Translate an Ocean.Request to a System.Net.HttpListenerResponse.
let private translateResponse (fxResponse : HttpListenerResponse) (oceanResponse : Response) =
    use sw = new StreamWriter(fxResponse.OutputStream)

    fxResponse.StatusCode <- oceanResponse.StatusCode
    fxResponse.StatusDescription <- oceanResponse.StatusMessage
    oceanResponse.Headers
    |> List.iter (fun header -> fxResponse.AddHeader(fst header, String.concat "," (snd header)))
    oceanResponse.BodyWriter(sw)

    sw.Close()

/// Resolve a request to a route handler pair.
let private resolveRoute (routes : RouteList) (notFound : RequestHandler) (req : Request) =
    let chooser (route : Route) =
        match (req |> fst route) with
        | Success p -> Some (snd route, Success p)
        | Failure -> None
    in
        let resolvedRoute = List.tryPick chooser routes
        match resolvedRoute with
        | Some s -> s
        | None -> notFound, Failure

/// Serve a RouteList on a given interface using the .NET Framework's integrated
/// web server. On Windows, consult the netsh documentation to find out how to
/// add a URL ACL to prevent permission errors while running as a non-elevated
/// user.
let serve (iface : string) (routes : RouteList) =
    Log.writef "[FrameworkWebServer] Serving on %s" iface

    // Set up listener.
    let httpListener = new HttpListener()
    httpListener.Start()
    httpListener.Prefixes.Add(iface)

    let processRequest () =
        // Wait for a request on this worker.
        let context = httpListener.GetContext()
        use streamWriter = new StreamWriter(context.Response.OutputStream)

        let request = translateRequest context
        Log.writef "[FrameworkWebServer] Accepting request for %s from %s"
            request.Url.AbsolutePath (request.RemoteEndPoint.ToString())

        // This needs to be cleaned up, we also don't want to call the
        // RouteMatcher function twice.
        let routeAndResult = resolveRoute routes (fun _ -> RespondWith.err 404) request
        let req2 =
            { request with MatchParameters =
                               match snd routeAndResult with
                               | Success p -> p
                               | Failure -> None }

        try
            req2 |> fst routeAndResult |> translateResponse context.Response
        with e ->
            Log.writef "[FrameworkWebServer] Unhandled exception in request handler:\n %s" (e.ToString())
            RespondWith.exn e |> translateResponse context.Response

        // Finish writing response.
        context.Response.Close()

    // Recursive accept loop.
    let rec acceptLoop () =
        try
            processRequest ()
            acceptLoop () // Recurse
        with e ->
            Log.writef "[FrameworkWebServer] Unhandled exception in listen loop:\n %s" (e.ToString())

    // Let's go!
    acceptLoop ()
