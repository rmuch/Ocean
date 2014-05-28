module Ocean.Tests.CoreTests

open System
open System.IO

open Ocean
open Ocean.Core

open NUnit.Framework

/// Test that the default response provides the expected error code and message.
[<Test>]
let TestResponseOKIsValid () =
    let response = Response.ok
    Assert.That(response.StatusCode, Is.EqualTo(200))
    Assert.That(response.StatusMessage, Is.StringMatching("OK"))

/// Test that the not found response provides the expected error code and message.
[<Test>]
let TestResponseErrorIsValid () =
    let response = Response.error 404
    Assert.That(response.StatusCode, Is.EqualTo(404))
    Assert.That(response.StatusMessage, Is.StringMatching("Not Found"))

/// Test that the string response writer writes the expected text to the output stream.
[<Test>]
let TestRespondWithStr () =
    use writer = new StringWriter()
    let testString = "hello world"
    let rsp = RespondWith.str testString
    rsp.BodyWriter(upcast writer)
    let writtenResult = writer.ToString()
    Assert.That(testString, Is.EqualTo(writtenResult))

/// Test that a full path is matched exactly.
[<Test>]
let TestMatchPath () =
    let testPath = "/test/path"
    let rootUrl = "http://localhost:8080"
    // Request that should pass.
    let matchResult = { Request.empty with Url = new Uri("http://localhost:8080/test/path") }
                      |> Match.path testPath
    match matchResult with
    | Success _ -> ()
    | Failure -> Assert.Fail()
    // Requests that should fail.
    [ "/test/path/"; "/test/path2" ]
    |> List.map (fun url -> { Request.empty with Url = new Uri(rootUrl + url) })
    |> List.iter (fun req -> match Match.path testPath req with
                             | Success _ -> Assert.Fail()
                             | Failure -> ())

/// Test that a full path is matched by prefix.
[<Test>]
let TestMatchPrefix () =
    let testPrefix = "/prefixed"
    let rootUrl = "http://localhost:8080"
    // Requests that should succeed.
    [ "/prefixed"; "/prefixed.jpg"; "/prefixed/path" ]
    |> List.map (fun url -> { Request.empty with Url = new Uri(rootUrl + url) })
    |> List.iter (fun req -> match Match.prefix testPrefix req with
                             | Success _ -> ()
                             | Failure -> Assert.Fail())
    // Requests that should fail.
    [ "/notprefixed"; "/prefixe" ]
    |> List.map (fun url -> { Request.empty with Url = new Uri(rootUrl + url) })
    |> List.iter (fun req -> match Match.prefix testPrefix req with
                             | Success _ -> Assert.Fail()
                             | Failure -> ())

/// Test that routes are resolved in ascending order.
[<Test>]
let TestResolveRouteOrder () =
    let order = ref 1
    let routes =
        [ // Route 1
          (fun req -> Assert.That(order, Is.EqualTo(1)); order := !order + 1; Match.path "/a" req),
          (fun _ -> Assert.Fail(); Response.ok)
          // Route 2
          (fun req -> Assert.That(order, Is.EqualTo(2)); order := !order + 1; Match.path "/b" req),
          (fun _ -> Response.ok)
          // Route 3
          (fun req -> Assert.That(order, Is.EqualTo(3)); Assert.Fail(); Match.path "/c" req),
          (fun _ -> Assert.Fail(); Response.ok) ]
    let req = { Request.empty with Url = new Uri("http://localhost:8080/b") }
    let result = resolveRoute routes (fun _ -> Assert.Fail(); Response.error 404) req
    req |> fst result |> ignore
