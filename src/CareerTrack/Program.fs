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

    // Kezdő oldal
    app.MapGet("/", Func<IResult>(fun () ->
        Results.Content("<h1>Hello World!</h1>", "text/html")
    )) |> ignore

    // JSON endpoint
    app.MapGet("/applications", Func<Application list>(fun () ->
        applications
    )) |> ignore

    // Lista oldal
    app.MapGet("/applications-page", Func<IResult>(fun () ->

        let rows =
            applications
            |> List.map (fun app ->

                let color =
                    match app.Status with
                    | "Applied" -> "green"
                    | "Interview" -> "orange"
                    | "Rejected" -> "red"
                    | _ -> "black"

                $"<tr>
                    <td>{app.Company}</td>
                    <td>{app.Position}</td>
                    <td style='color:{color}; font-weight:bold;'>{app.Status}</td>
                </tr>"
            )
            |> String.concat ""

        let html = $"""
<html>
<head>
    <meta charset="UTF-8">
    <title>CareerTrack Applications</title>
</head>
<body>

<h1 style="text-align:center;">Álláspályázatok</h1>

<table border="1" cellpadding="10" style="margin:auto;">
    <tr>
        <th>Vállalat</th>
        <th>Pozíció</th>
        <th>Állapot</th>
    </tr>

    {rows}

</table>

<br/>

<a href="/add-application">Új alkalmazás hozzáadása</a>

</body>
</html>
"""

        Results.Content(html, "text/html")
    )) |> ignore

    // Űrlap oldal
    app.MapGet("/add-application", Func<IResult>(fun () ->

        let html = """
<html>
<head>
    <meta charset="UTF-8">
    <title>Új alkalmazás</title>
</head>
<body>

<h1>Új alkalmazás hozzáadása</h1>

<form method="post" action="/applications">

<div>
<label>Vállalat:</label><br/>
<input type="text" name="company"/>
</div>

<br/>

<div>
<label>Pozíció:</label><br/>
<input type="text" name="position"/>
</div>

<br/>

<div>
<label>Állapot:</label><br/>
<input type="text" name="status"/>
</div>

<br/>

<div>
<label>Megjegyzések:</label><br/>
<textarea name="notes"></textarea>
</div>

<br/>

<button type="submit">Alkalmazás mentése</button>

</form>

<br/>

<a href="/applications-page">Vissza a listához</a>

</body>
</html>
"""

        Results.Content(html, "text/html")
    )) |> ignore

    // POST feldolgozás
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
    <meta charset="UTF-8">
    <title>Siker</title>
</head>
<body>

<h1>Alkalmazás mentve</h1>

<p><b>Vállalat:</b> {company}</p>
<p><b>Pozíció:</b> {position}</p>
<p><b>Állapot:</b> {status}</p>
<p><b>Megjegyzések:</b> {notes}</p>

<br/>

<a href="/applications-page">Vissza a listához</a>

</body>
</html>
"""

        Results.Content(html, "text/html")
    )) |> ignore

    app.Run()

    0