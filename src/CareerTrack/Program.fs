open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Http
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

    app.MapGet("/", Func<IResult>(fun () ->
        Results.Content("<h1>Hello World!</h1>", "text/html")
    )) |> ignore

    app.MapGet("/applications", Func<Application list>(fun () ->
        applications
    )) |> ignore

    app.MapGet("/applications-page", Func<IResult>(fun () ->

        let rows =
            applications
            |> List.map (fun app ->
                $"<tr>
                    <td>{app.Company}</td>
                    <td>{app.Position}</td>
                    <td>{app.Status}</td>
                </tr>")
            |> String.concat ""

        let html = $"""
<html>
<head>
    <title>CareerTrack Applications</title>
</head>
<body>

<h1>Job Applications</h1>

<table border="1" cellpadding="10">
    <tr>
        <th>Company</th>
        <th>Position</th>
        <th>Status</th>
    </tr>

    {rows}

</table>

<br/>

<a href="/add-application">Add new application</a>

</body>
</html>
"""

        Results.Content(html, "text/html")
    )) |> ignore

    app.MapGet("/add-application", Func<IResult>(fun () ->

        let html = """
<html>
<head>
<title>Add Application</title>
</head>
<body>

<h1>Add New Application</h1>

<form method="post" action="/applications">

<div>
<label>Company:</label><br/>
<input type="text" name="company"/>
</div>

<br/>

<div>
<label>Position:</label><br/>
<input type="text" name="position"/>
</div>

<br/>

<div>
<label>Status:</label><br/>
<input type="text" name="status"/>
</div>

<br/>

<div>
<label>Notes:</label><br/>
<textarea name="notes"></textarea>
</div>

<br/>

<button type="submit">Save Application</button>

</form>

<br/>

<a href="/applications-page">Back to list</a>

</body>
</html>
"""

        Results.Content(html, "text/html")
    )) |> ignore

    app.MapPost("/applications", Func<HttpRequest, IResult>(fun request ->

        let company =
            if request.Form.ContainsKey("company") then request.Form["company"].ToString() else ""

        let position =
            if request.Form.ContainsKey("position") then request.Form["position"].ToString() else ""

        let status =
            if request.Form.ContainsKey("status") then request.Form["status"].ToString() else ""

        let notes =
            if request.Form.ContainsKey("notes") then request.Form["notes"].ToString() else ""

        let html = $"""
<html>
<head>
<title>Application Saved</title>
</head>
<body>

<h1>Application received</h1>

<p><b>Company:</b> {company}</p>
<p><b>Position:</b> {position}</p>
<p><b>Status:</b> {status}</p>
<p><b>Notes:</b> {notes}</p>

<br/>

<a href="/applications-page">Back to applications</a>

</body>
</html>
"""

        Results.Content(html, "text/html")
    )) |> ignore

    app.Run()

    0