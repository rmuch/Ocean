# Ocean

## Introduction

Ocean is a new web framework designed to make it easy to write web applications in F#.

Ocean's philosophy is to provide just what you need to do web development in an expressive functional style in F#. It's not opinionated about your design decisions. It plays nicely with existing .NET libraries and technologies. It seeks to provide clear tools that don't challenge your assumptions.

Ocean draws inspiration from many of the newest popular lightweight web frameworks, including, but not limited to Go's http package and Gorilla toolkit, C#'s Nancy and Ruby's Sinatra, Python's Flask and web.py, Haskell's Snap and the multitude of other minimalistic web frameworks.

## Show me the code!

Here is a simple website in Ocean, demonstrating how easy it is to get started: -

```fsharp
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
```

## Getting Started

### Obtaining and building sources

To get started with Ocean, obtain a copy of the source code from the Git repository. You can use the `git clone` command, or download a zip bundle from the website.

Use MonoDevelop or Visual Studio (or alternatively, the command line tool `xbuild` from Mono or Microsoft `msbuild`) to compile the solution.

Add a reference to Ocean.dll in your project. You are now ready to start writing code.

### Windows: Using netsh to configure permissions

When you attempt to build and run the Ocean sample application on a Windows system, you may be faced with a permission error preventing you from running the application with non-elevated permissions.

You can use the `netsh` command to add a URL ACL reservation, to allow a non-elevated user to serve HTTP on a specific hostname and port.

For example, to add a URL ACL reservation for the local user Jim to serve on port 8080, use the following command: -

    netsh add urlacl url=http://*:8080/ user=\Jim

For more information on the `netsh` command, see the following MSDN topics: -

 * [Netsh commands for HTTP: add urlacl](http://msdn.microsoft.com/en-us/library/windows/desktop/cc307223.aspx)
 * [HTTP.sys Manageability in Windows Vista and Windows Server 2008](http://msdn.microsoft.com/en-us/library/windows/desktop/cc308384.aspx)

### Linux and Mac OS X with Mono: Notice

At the time of writing, Mono has only recently gained support the new MSBuild format used by Ocean's project files. You may need to obtain a more recent version of Mono, build Mono from HEAD or create alternative build files for Ocean if compiling on Mac OS X or Linux.
