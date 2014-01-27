module Ocean.Tests.CoreTests

open System
open System.IO

open Ocean
open Ocean.Core

open NUnit.Framework

[<Test>]
let TestResponseOKIsValid () =
    let response = Response.ok
    Assert.That(response.StatusCode, Is.EqualTo(200))
    Assert.That(response.StatusMessage, Is.StringMatching("OK"))

[<Test>]
let TestResponseErrorIsValid () =
    let response = Response.error 404
    Assert.That(response.StatusCode, Is.EqualTo(404))
    Assert.That(response.StatusMessage, Is.StringMatching("Not Found"))

[<Test>]
let TestMatchPath () =
    let testPath = "/test/path"
    let rootUrl = "http://localhost:8080"
    // Request that should pass.
    let matchResult = Match.path testPath { Request.empty with Url = new Uri("http://localhost:8080/test/path") }
    match matchResult with
    | Success _ -> ()
    | Failure -> Assert.Fail()
    // Requests that should fail.
    [ "/test/path/"; "/test/path2" ]
    |> List.map (fun url -> { Request.empty with Url = new Uri(rootUrl + url) })
    |> List.iter (fun req -> match Match.path testPath req with
                             | Success _ -> Assert.Fail()
                             | Failure -> ())

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

[<Test>]
let TestResolveRouteOrder () =
    let order = ref 1
    let routes =
        [ (fun req -> Assert.That(order, Is.EqualTo(1)); order := !order + 1; Match.path "/a" req), fun _ -> Assert.Fail(); Response.ok
          (fun req -> Assert.That(order, Is.EqualTo(2)); order := !order + 1; Match.path "/b" req), fun _ -> Response.ok
          (fun req -> Assert.That(order, Is.EqualTo(3)); Assert.Fail(); Match.path "/c" req), fun _ -> Assert.Fail(); Response.ok ]
    let req = { Request.empty with Url = new Uri("http://localhost:8080/b") }
    let result = resolveRoute routes (fun _ -> Assert.Fail(); Response.error 404) req
    req |> fst result |> ignore
