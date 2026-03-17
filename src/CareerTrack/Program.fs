open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open CareerTrack.Models

[<EntryPoint>]
let main args =

    let builder = WebApplication.CreateBuilder(args)
    let app = builder.Build()

    let applications : Application list = [
        {
            Id = 1
            Company = "Microsoft"
            Position = "Backend Intern"
            DateApplied = DateTime(2026, 3, 1)
            Status = "Applied"
            Notes = "Applied through company website"
        }
        {
            Id = 2
            Company = "Google"
            Position = "Software Engineer Intern"
            DateApplied = DateTime(2026, 3, 3)
            Status = "Interview"
            Notes = "HR screening completed"
        }
        {
            Id = 3
            Company = "SAP"
            Position = "Junior Developer"
            DateApplied = DateTime(2026, 3, 5)
            Status = "Rejected"
            Notes = "Received rejection email"
        }
    ]

    app.MapGet("/", Func<string>(fun () ->
        "Hello World!"
    )) |> ignore

    app.MapGet("/applications", Func<Application list>(fun () ->
        applications
    )) |> ignore

    app.MapGet("/applications-page", Func<string>(fun () ->

        let items =
            applications
            |> List.map (fun app -> $"<li>{app.Company} - {app.Position} ({app.Status})</li>")
            |> String.concat ""

        $"<html>
            <head>
                <title>CareerTrack Applications</title>
            </head>
            <body>
                <h1>Job Applications</h1>
                <ul>
                    {items}
                </ul>
            </body>
        </html>"
    )) |> ignore

    app.Run()

    0
