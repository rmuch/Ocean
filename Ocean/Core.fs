﻿/// Core module of the Ocean web framework, managing routing.
module Ocean.Core

open System
open System.IO
open System.Net
open System.Text.RegularExpressions

/// Takes a Response (or any type 'E) and returns a RequestHandler (or any
/// genericized function of 'P -> 'E), discarding the parameter 'P and
/// returning 'E.
let ignoreRequest<'E, 'P> (e : 'E) (_ : 'P) : 'E = e
let internal ignoreFirst = ignoreRequest

/// Type representing a HTTP cookie received from or about to be sent in
/// response to a request.
type Cookie =
    { /// Cookie name.
      Name : string
      /// Cookie value.
      Value : string
      /// Cookie expiry date.
      Expiry : DateTime }

/// Type representing a HTTP request received from a client and ready to be
/// processed by a RequestHandler.
type Request =
    { /// HTTP request verb.
      Verb : string
      /// Target URL.
      Url : Uri
      /// HTTP headers.
      Headers : (string * string list) list
      /// Cookies.
      Cookies : Cookie list
      /// Body reader.
      BodyReader : unit -> StreamReader
      /// IP address and port of the request origin.
      RemoteEndPoint : IPEndPoint
      /// Set of match parameters generated by the route's RouteMatcher before
      /// being routed to the final RequestHandler.
      MatchParameters : (string * string) list option }
    /// An empty request definition.
    static member empty =
        { Verb = ""
          Url = null
          Headers = []
          Cookies = []
          BodyReader = null |> ignoreFirst
          RemoteEndPoint = null
          MatchParameters = None }

/// Type representing a HTTP response to be generated by a RequestHandler.
type Response =
    { /// HTTP status code, i.e. 200 (OK).
      StatusCode : int
      /// HTTP status message, i.e. "200 OK".
      StatusMessage : string
      /// HTTP headers.
      Headers : (string * string list) list
      /// Cookies.
      Cookies : Cookie list
      /// Body writer.
      BodyWriter : StreamWriter -> unit }
    /// A default, empty response definition serving 200 OK and no content.
    static member empty =
        { StatusCode = 200
          StatusMessage = "200 OK"
          Headers = []
          Cookies = []
          BodyWriter = () |> ignoreFirst }
    /// A default, empty response definition serving 200 OK and no content.
    static member ok = Response.empty
    /// A response to serve an error code.
    static member error (code : int) =
        let errorCodes = dict [ 404, "Not Found"; 500, "Internal Server Error" ]
        let writer (w : StreamWriter) =
            w.WriteLine("<h1>{0} {1}</h1>", code, errorCodes.Item code)
        let msg = (code.ToString()) + " " + errorCodes.Item code
        { Response.empty with StatusCode = code; StatusMessage = msg; BodyWriter = writer }

/// Result type for a RouteMatcher predicate function, representing either a
/// successful match with optional list parameters, or a failure to provide
/// a match.
type MatchResult =
    | Success of (string * string) list option
    | Failure

/// Predicate function type to associate a request with a route, accepting a
/// HTTP request as a parameter and returning a boolean value indicating a
/// successful match.
type RouteMatcher = Request -> MatchResult

/// Function signature of a request handler, accpeting a request parameter and
/// returning a response to be written to the client.
type RequestHandler = Request -> Response

/// Tuple of a route match predicate and a request handler.
type Route = RouteMatcher * RequestHandler

/// List of pairs of route matchers and request handlers.
type RouteList = Route list

/// Basic URL-matching predicate functions.
module Match =
    /// Match a path exactly.
    let path (path : string) (req : Request) : MatchResult =
        if req.Url.AbsolutePath = path then Success None else Failure
    /// Match a path with a specific prefix.
    let prefix (path : string) (req : Request) : MatchResult =
        if req.Url.AbsolutePath.StartsWith(path) then Success None else Failure
    /// Match by regular expression.
    let regex (pattern : string) (req : Request) : MatchResult =
        let rgx = Regex.Match(req.Url.AbsolutePath, pattern)
        if rgx.Success then Success None else Failure

/// Helper functions for quick responses.
module RespondWith =
    /// Generate a response serving a string.
    let str (s : string) : Response =
        { Response.ok with BodyWriter = fun w -> w.Write(s) }
    /// Generate a response serving a file.
    let file (path : string) : Response =
        { Response.ok with BodyWriter = fun w -> w.Write(File.ReadAllText(path)) }
    /// Generate a response serving a default page for a HTTP error code.
    let err (err : int) : Response =
        Response.error err
    /// Generate a response serving a default page for an exception and stack trace.
    let exn (ex : exn) : Response =
        let exnWriter (w : StreamWriter) =
            w.WriteLine("<h1>" + ex.Message + "</h1>")
            w.WriteLine("<pre>" + ex.ToString() + "</pre>")
        { Response.error 500 with BodyWriter = exnWriter }
