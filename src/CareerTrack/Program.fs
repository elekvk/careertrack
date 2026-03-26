open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Http

type Application =
    {
        Id: int
        Company: string
        Position: string
        DateApplied: DateTime
        Status: string
        Notes: string
    }

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    let app = builder.Build()

    let htmlPage title body =
        Results.Content(
            "<html>" +
            "<head>" +
            "<meta charset=\"UTF-8\">" +
            "<title>" + title + "</title>" +
            "<style>" +
            "body { font-family: Arial; background-color: #f4f6f8; margin:0; padding:0; }" +
            "h1 { text-align:center; }" +
            ".container { width: 80%; margin: auto; padding: 20px; }" +
            "table { width: 100%; border-collapse: collapse; background:white; box-shadow:0 2px 8px rgba(0,0,0,0.1); }" +
            "th, td { padding: 10px; border-bottom: 1px solid #ddd; text-align:left; }" +
            "th { background-color: #2c3e50; color:white; }" +
            "tr:hover { background-color: #f1f1f1; }" +
            "a { text-decoration:none; color:#3498db; font-weight:bold; }" +
            "a:hover { text-decoration:underline; }" +
            ".btn { display:inline-block; padding:10px 15px; background:#3498db; color:white; border-radius:5px; border:none; cursor:pointer; }" +
            ".btn:hover { background:#2980b9; }" +
            "input, select, textarea { padding:8px; width:300px; margin-bottom:10px; }" +
            ".stats { text-align:center; margin-bottom:20px; font-size:18px; }" +
            "</style>" +
            "</head>" +
            "<body><div class=\"container\">" +
            body +
            "</div></body></html>",
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
            Notes = "Applied through website"
        }
    )

    applications.Add(
        {
            Id = 2
            Company = "Google"
            Position = "Software Engineer Intern"
            DateApplied = DateTime(2026, 3, 3)
            Status = "Interview"
            Notes = "HR round done"
        }
    )

    applications.Add(
        {
            Id = 3
            Company = "SAP"
            Position = "Junior Developer"
            DateApplied = DateTime(2026, 3, 5)
            Status = "Rejected"
            Notes = "Rejected email"
        }
    )

    app.MapGet("/", Func<IResult>(fun () ->
        let body =
            "<h1>CareerTrack</h1>" +
            "<p style=\"text-align:center;\"><a href=\"/applications-page\">Go to applications</a></p>"

        htmlPage "Home" body
    )) |> ignore

    app.MapGet("/applications", Func<Application list>(fun () ->
        applications |> Seq.toList
    )) |> ignore

    app.MapGet("/applications-page", Func<HttpContext, IResult>(fun ctx ->
        let search = ctx.Request.Query["search"].ToString().Trim().ToLower()
        let statusFilter = ctx.Request.Query["status"].ToString()

        let filtered =
            applications
            |> Seq.filter (fun a ->
                let matchesSearch =
                    if String.IsNullOrWhiteSpace(search) then
                        true
                    else
                        a.Company.ToLower().Contains(search)
                        || a.Position.ToLower().Contains(search)
                        || a.Status.ToLower().Contains(search)
                        || a.Notes.ToLower().Contains(search)

                let matchesStatus =
                    if String.IsNullOrWhiteSpace(statusFilter) then
                        true
                    else
                        a.Status = statusFilter

                matchesSearch && matchesStatus
            )

        let appliedCount =
            filtered |> Seq.filter (fun a -> a.Status = "Applied") |> Seq.length

        let interviewCount =
            filtered |> Seq.filter (fun a -> a.Status = "Interview") |> Seq.length

        let rejectedCount =
            filtered |> Seq.filter (fun a -> a.Status = "Rejected") |> Seq.length

        let rows =
            filtered
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
                "<td><span style=\"background:" + color + ";color:white;padding:4px 8px;border-radius:5px;\">" + a.Status + "</span></td>" +
                "<td>" + a.DateApplied.ToString("yyyy-MM-dd") + "</td>" +
                "<td>" + a.Notes + "</td>" +
                "<td><a href=\"/application/" + string a.Id + "\">View</a> | <a href=\"/edit/" + string a.Id + "\">Edit</a> | <a href=\"/delete/" + string a.Id + "\" onclick=\"return confirm('Are you sure?')\">Delete</a></td>" +
                "</tr>"
            )
            |> String.concat ""

        let body =
            "<h1>Job Applications</h1>" +
            "<div class=\"stats\">" +
            "<span style=\"color:green; font-weight:bold;\">Applied: " + string appliedCount + "</span> | " +
            "<span style=\"color:orange; font-weight:bold;\">Interview: " + string interviewCount + "</span> | " +
            "<span style=\"color:red; font-weight:bold;\">Rejected: " + string rejectedCount + "</span>" +
            "</div>" +
            "<div style=\"text-align:center;margin-bottom:20px;\">" +
            "<a class=\"btn\" href=\"/add-application\">+ Add Application</a>" +
            "</div>" +
            "<form method=\"get\" action=\"/applications-page\" style=\"text-align:center;margin-bottom:20px;\">" +
            "<input name=\"search\" value=\"" + search + "\" placeholder=\"Search\" /> " +
            "<select name=\"status\">" +
            "<option value=\"\">All</option>" +
            "<option value=\"Applied\">Applied</option>" +
            "<option value=\"Interview\">Interview</option>" +
            "<option value=\"Rejected\">Rejected</option>" +
            "</select> " +
            "<button class=\"btn\" type=\"submit\">Filter</button> " +
            "<a class=\"btn\" href=\"/applications-page\">Clear</a>" +
            "</form>" +
            "<table>" +
            "<tr><th>Company</th><th>Position</th><th>Status</th><th>Date</th><th>Notes</th><th>Action</th></tr>" +
            rows +
            "</table>"

        htmlPage "Applications" body
    )) |> ignore

    app.MapGet("/application/{id}", Func<int, IResult>(fun id ->
        let item =
            applications |> Seq.tryFind (fun a -> a.Id = id)

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
                "<p><b>Status:</b> <span style=\"background:" + color + ";color:white;padding:4px 8px;border-radius:5px;\">" + a.Status + "</span></p>" +
                "<p><b>Date applied:</b> " + a.DateApplied.ToString("yyyy-MM-dd") + "</p>" +
                "<p><b>Notes:</b> " + a.Notes + "</p>" +
                "<p><a href=\"/applications-page\">Back to list</a></p>"

            htmlPage "Details" body
        | None ->
            htmlPage "Not found" "<h1>Application not found</h1><p><a href=\"/applications-page\">Back to list</a></p>"
    )) |> ignore

    app.MapGet("/add-application", Func<IResult>(fun () ->
        let body =
            "<h1>Add Application</h1>" +
            "<form method=\"post\" action=\"/applications\">" +
            "<input name=\"company\" placeholder=\"Company\" /><br/>" +
            "<input name=\"position\" placeholder=\"Position\" /><br/>" +
            "<select name=\"status\">" +
            "<option value=\"Applied\">Applied</option>" +
            "<option value=\"Interview\">Interview</option>" +
            "<option value=\"Rejected\">Rejected</option>" +
            "</select><br/>" +
            "<textarea name=\"notes\" placeholder=\"Notes\"></textarea><br/>" +
            "<button class=\"btn\" type=\"submit\">Save</button>" +
            "</form>" +
            "<p><a href=\"/applications-page\">Back to list</a></p>"

        htmlPage "Add Application" body
    )) |> ignore

    app.MapPost("/applications", Func<HttpRequest, IResult>(fun req ->
        let get name =
            if req.Form.ContainsKey(name) then req.Form[name].ToString()
            else ""

        let newApp =
            {
                Id = applications.Count + 1
                Company = get "company"
                Position = get "position"
                DateApplied = DateTime.Now
                Status = get "status"
                Notes = get "notes"
            }

        applications.Add(newApp)
        Results.Redirect("/applications-page")
    )) |> ignore

    app.MapGet("/edit/{id}", Func<int, IResult>(fun id ->
        let item =
            applications |> Seq.tryFind (fun a -> a.Id = id)

        match item with
        | Some a ->
            let selectedApplied = if a.Status = "Applied" then "selected" else ""
            let selectedInterview = if a.Status = "Interview" then "selected" else ""
            let selectedRejected = if a.Status = "Rejected" then "selected" else ""

            let body =
                "<h1>Edit Application</h1>" +
                "<form method=\"post\" action=\"/update/" + string a.Id + "\">" +
                "<input name=\"company\" value=\"" + a.Company + "\" /><br/>" +
                "<input name=\"position\" value=\"" + a.Position + "\" /><br/>" +
                "<select name=\"status\">" +
                "<option value=\"Applied\" " + selectedApplied + ">Applied</option>" +
                "<option value=\"Interview\" " + selectedInterview + ">Interview</option>" +
                "<option value=\"Rejected\" " + selectedRejected + ">Rejected</option>" +
                "</select><br/>" +
                "<textarea name=\"notes\">" + a.Notes + "</textarea><br/>" +
                "<button class=\"btn\" type=\"submit\">Update</button>" +
                "</form>" +
                "<p><a href=\"/applications-page\">Back to list</a></p>"

            htmlPage "Edit Application" body
        | None ->
            htmlPage "Not found" "<h1>Application not found</h1><p><a href=\"/applications-page\">Back to list</a></p>"
    )) |> ignore

    app.MapPost("/update/{id}", Func<int, HttpRequest, IResult>(fun id req ->
        let get name fallback =
            if req.Form.ContainsKey(name) then req.Form[name].ToString()
            else fallback

        let existing =
            applications |> Seq.tryFind (fun a -> a.Id = id)

        match existing with
        | Some oldItem ->
            let index = applications |> Seq.findIndex (fun a -> a.Id = id)

            applications.[index] <-
                {
                    Id = oldItem.Id
                    Company = get "company" oldItem.Company
                    Position = get "position" oldItem.Position
                    DateApplied = oldItem.DateApplied
                    Status = get "status" oldItem.Status
                    Notes = get "notes" oldItem.Notes
                }

            Results.Redirect("/applications-page")
        | None ->
            htmlPage "Not found" "<h1>Application not found</h1><p><a href=\"/applications-page\">Back to list</a></p>"
    )) |> ignore

    app.MapGet("/delete/{id}", Func<int, IResult>(fun id ->
        applications.RemoveAll(fun a -> a.Id = id) |> ignore
        Results.Redirect("/applications-page")
    )) |> ignore

    app.Run()
    0