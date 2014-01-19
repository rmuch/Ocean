/// Provides basic logging facilities.
module Ocean.Log

open System

/// Return a current log-formatted timestamp.
let internal getTimeStamp () =
    let now = DateTime.Now
    in now.ToShortDateString() + " " + now.ToShortTimeString() 

/// Write a log message.
let write (msg : string) =
    printfn "[%s] %s" (getTimeStamp ()) msg

/// Write a formatted log message.
let writef (fmt : Printf.StringFormat<_, _>) =
    Printf.ksprintf write fmt
