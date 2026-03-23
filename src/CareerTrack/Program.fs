open System
open System.Collections.Generic
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Http
open CareerTrack.Models

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    let app = builder.Build()

    let htmlPage title body =
        Results.Content(
            "<html><head><meta charset=\"UTF-8\"><title>" + title + "</title></head><body>" + body + "</body></html>",
            "text/html; charset=utf-8"
        )

    let applications = ResizeArray<Application>()

    applications.Add(
        {
            Id = 1
            Company = "Microsoft"
            Position = "Backend Intern"
            DateApplied = DateTime(2026, 3, 1)
            Status = "Applied"
            Notes = "Applied through company website"
        }
    )

    applications.Add(
        {
            Id = 2
            Company = "Google"
            Position = "Software Engineer Intern"
            DateApplied = DateTime(2026, 3, 3)
            Status = "Interview"
            Notes = "HR screening completed"
        }
    )

    applications.Add(
        {
            Id = 3
            Company = "SAP"
            Position = "Junior Developer"
            DateApplied = DateTime(2026, 3, 5)
            Status = "Rejected"
            Notes = "Received rejection email"
        }
    )

    app.MapGet("/", Func<IResult>(fun () ->
        htmlPage
            "CareerTrack"
            "<h1>Hello World!</h1><p><a href=\"/applications-page\">Go to applications</a></p>"
    )) |> ignore

    app.MapGet("/applications", Func<Application list>(fun () ->
        applications |> Seq.toList
    )) |> ignore

    app.MapGet("/applications-page", Func<IResult>(fun () ->
        let rows =
            applications
            |> Seq.map (fun a ->
                let color =
                    match a.Status with
                    | "Applied" -> "green"
                    | "Interview" -> "orange"
                    | "Rejected" -> "red"
                    | _ -> "black"

                "<tr>" +
                "<td>" + a.Company + "</td>" +
                "<td>" + a.Position + "</td>" +
                "<td style=\"color:" + color + "; font-weight:bold;\">" + a.Status + "</td>" +
                "<td>" + a.DateApplied.ToString("yyyy-MM-dd") + "</td>" +
                "<td>" + a.Notes + "</td>" +
                "<td><a href=\"/application/" + string a.Id + "\">View</a> | <a href=\"/delete/" + string a.Id + "\">Delete</a></td>" +
                "</tr>"
            )
            |> String.concat ""

        let body =
            "<h1 style=\"text-align:center;\">Job Applications</h1>" +
            "<div style=\"text-align:center; margin-bottom:20px;\"><a href=\"/add-application\">Add new application</a></div>" +
            "<table border=\"1\" cellpadding=\"10\" style=\"margin:auto; border-collapse:collapse;\">" +
            "<tr><th>Company</th><th>Position</th><th>Status</th><th>Date</th><th>Notes</th><th>Action</th></tr>" +
            rows +
            "</table>"

        htmlPage "Applications" body
    )) |> ignore

    app.MapGet("/application/{id}", Func<int, IResult>(fun id ->
        let item =
            applications
            |> Seq.tryFind (fun a -> a.Id = id)

        match item with
        | Some a ->
            let color =
                match a.Status with
                | "Applied" -> "green"
                | "Interview" -> "orange"
                | "Rejected" -> "red"
                | _ -> "black"

            let body =
                "<h1>Application Details</h1>" +
                "<p><b>Company:</b> " + a.Company + "</p>" +
                "<p><b>Position:</b> " + a.Position + "</p>" +
                "<p><b>Status:</b> <span style=\"color:" + color + "; font-weight:bold;\">" + a.Status + "</span></p>" +
                "<p><b>Date applied:</b> " + a.DateApplied.ToString("yyyy-MM-dd") + "</p>" +
                "<p><b>Notes:</b> " + a.Notes + "</p>" +
                "<br/>" +
                "<a href=\"/applications-page\">Back to list</a>"

            htmlPage "Application Details" body
        | None ->
            htmlPage
                "Not Found"
                "<h1>Not Found</h1><p>Application not found.</p><a href=\"/applications-page\">Back to list</a>"
    )) |> ignore

    app.MapGet("/add-application", Func<IResult>(fun () ->
        let body =
            "<h1>Add New Application</h1>" +
            "<form method=\"post\" action=\"/applications\">" +
            "<div><label>Company:</label><br/><input type=\"text\" name=\"company\" /></div><br/>" +
            "<div><label>Position:</label><br/><input type=\"text\" name=\"position\" /></div><br/>" +
            "<div><label>Status:</label><br/>" +
            "<select name=\"status\">" +
            "<option value=\"Applied\">Applied</option>" +
            "<option value=\"Interview\">Interview</option>" +
            "<option value=\"Rejected\">Rejected</option>" +
            "</select></div><br/>" +
            "<div><label>Notes:</label><br/><textarea name=\"notes\"></textarea></div><br/>" +
            "<button type=\"submit\">Save Application</button>" +
            "</form><br/>" +
            "<a href=\"/applications-page\">Back to list</a>"

        htmlPage "Add Application" body
    )) |> ignore

    app.MapPost("/applications", Func<HttpRequest, IResult>(fun request ->
        let getField name defaultValue =
            if request.Form.ContainsKey(name) then
                request.Form[name].ToString().Trim()
            else
                defaultValue

        let company = getField "company" ""
        let position = getField "position" ""
        let status = getField "status" "Applied"
        let notes = getField "notes" ""

        if String.IsNullOrWhiteSpace(company) || String.IsNullOrWhiteSpace(position) then
            htmlPage
                "Error"
                "<h1>Error</h1><p>Company and Position are required.</p><a href=\"/add-application\">Back</a>"
        else
            let newId =
                if applications.Count = 0 then
                    1
                else
                    (applications |> Seq.map (fun a -> a.Id) |> Seq.max) + 1

            let newApplication =
                {
                    Id = newId
                    Company = company
                    Position = position
                    DateApplied = DateTime.Now
                    Status = status
                    Notes = notes
                }

            applications.Add(newApplication)
            Results.Redirect("/applications-page")
    )) |> ignore

    app.MapGet("/delete/{id}", Func<int, IResult>(fun id ->
        let itemToRemove =
            applications
            |> Seq.tryFind (fun a -> a.Id = id)

        match itemToRemove with
        | Some appToDelete ->
            applications.Remove(appToDelete) |> ignore
            Results.Redirect("/applications-page")
        | None ->
            htmlPage
                "Not Found"
                "<h1>Not Found</h1><p>Application not found.</p><a href=\"/applications-page\">Back to list</a>"
    )) |> ignore

    app.Run()
    0