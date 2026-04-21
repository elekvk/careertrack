open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Http
open Domain

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
            "body, input, select, textarea, button { font-family: Arial, sans-serif; }" +
            "h1 { text-align:center; color:#2c3e50; }" +
            "h2 { color:#2c3e50; }" +
            ".container { width: 80%; margin: auto; padding: 20px; }" +
            "table { width: 100%; border-collapse: collapse; background:white; box-shadow:0 2px 8px rgba(0,0,0,0.1); }" +
            "th, td { padding: 10px; border-bottom: 1px solid #ddd; text-align:left; vertical-align:top; }" +
            "th { background-color: #2c3e50; color:white; }" +
            "tr:hover { background-color: #f1f1f1; }" +
            "a { text-decoration:none; color:#3498db; font-weight:bold; }" +
            "a:hover { text-decoration:underline; }" +
            ".btn { display:inline-block; padding:10px 15px; background:#3498db; color:white; border-radius:5px; border:none; cursor:pointer; margin:2px; }" +
            ".btn:hover { background:#2980b9; }" +
            "input, select, textarea { padding:8px; width:300px; margin-bottom:10px; }" +
            "textarea { min-height:100px; }" +
            ".stats { text-align:center; margin-bottom:20px; font-size:18px; }" +
            ".message { text-align:center; font-weight:bold; padding:10px; border-radius:6px; margin-bottom:20px; }" +
            ".success { color:green; background:#eafaf1; }" +
            ".error { color:#b00020; background:#fdecea; }" +
            ".empty-message { text-align:center; background:white; padding:20px; border-radius:8px; box-shadow:0 2px 8px rgba(0,0,0,0.1); font-weight:bold; color:#555; }" +
            ".highlight-box { background:#e8f5e9; padding:15px; border-radius:10px; margin-bottom:20px; text-align:center; box-shadow:0 2px 8px rgba(0,0,0,0.08); }" +
            ".progress-bar { height:20px; border-radius:8px; transition: width 0.8s ease-in-out; }" +
            ".card { display:inline-block; width:180px; margin:10px; padding:20px; background:white; border-radius:12px; box-shadow:0 2px 8px rgba(0,0,0,0.08); text-align:center; vertical-align:top; }" +
            ".card-title { font-size:14px; color:#666; margin-bottom:8px; }" +
            ".card-value { font-size:28px; font-weight:bold; }" +
            ".favorites-box { background:#fff8e1; padding:16px; border-radius:10px; margin-top:20px; box-shadow:0 2px 8px rgba(0,0,0,0.06); }" +
            ".recent-item { background:white; padding:12px 16px; margin-bottom:10px; border-radius:10px; box-shadow:0 2px 8px rgba(0,0,0,0.06); }" +
            ".subtitle { text-align:center; font-size:18px; color:#555; margin-top:-8px; margin-bottom:24px; }" +
            "</style>" +
            "</head>" +
            "<body><div class=\"container\">" +
            body +
            "</div></body></html>",
            "text/html; charset=utf-8"
        )

    let statusToString status =
        match status with
        | Applied -> "Applied"
        | Interview -> "Interview"
        | Rejected -> "Rejected"

    let statusColor status =
        match status with
        | Applied -> "green"
        | Interview -> "orange"
        | Rejected -> "red"

    let cardHtml title value color =
        "<div class=\"card\">" +
        "<div class=\"card-title\">" + title + "</div>" +
        "<div class=\"card-value\" style=\"color:" + color + ";\">" + value + "</div>" +
        "</div>"

    let applications = ResizeArray<Application>()

    let getNextId () =
        if applications.Count = 0 then
            1
        else
            applications
            |> Seq.map (fun a -> a.Id)
            |> Seq.max
            |> fun maxId -> maxId + 1

    applications.Add(
        {
            Id = 1
            Company = "Microsoft"
            Position = "Backend Intern"
            DateApplied = DateTime(2026, 3, 1)
            Status = Applied
            Notes = "Applied through website"
            IsFavorite = false
        }
    )

    applications.Add(
        {
            Id = 2
            Company = "Google"
            Position = "Software Engineer Intern"
            DateApplied = DateTime(2026, 3, 3)
            Status = Interview
            Notes = "HR round done"
            IsFavorite = true
        }
    )

    applications.Add(
        {
            Id = 3
            Company = "SAP"
            Position = "Junior Developer"
            DateApplied = DateTime(2026, 3, 5)
            Status = Rejected
            Notes = "Rejected email"
            IsFavorite = false
        }
    )

    app.MapGet("/", Func<IResult>(fun () ->
        let body =
            "<h1>CareerTrack</h1>" +
            "<div class=\"subtitle\">Track your job and internship applications in one place</div>" +
            "<div style=\"text-align:center; margin-top:20px;\">" +
            "<a class=\"btn\" href=\"/applications-page\">Go to applications</a>" +
            "<a class=\"btn\" href=\"/stats\">View statistics</a>" +
            "</div>"

        htmlPage "Home" body
    )) |> ignore

    app.MapGet("/applications", Func<Application list>(fun () ->
        applications |> Seq.toList
    )) |> ignore

    app.MapGet("/favorite/{id}", Func<int, IResult>(fun id ->
        let existing = applications |> Seq.tryFind (fun a -> a.Id = id)

        match existing with
        | Some item ->
            let index = applications |> Seq.findIndex (fun a -> a.Id = id)
            applications.[index] <- { item with IsFavorite = not item.IsFavorite }
            Results.Redirect("/applications-page?success=Favorite updated successfully")
        | None ->
            Results.Redirect("/applications-page?error=Application not found")
    )) |> ignore

    app.MapGet("/applications-page", Func<HttpContext, IResult>(fun ctx ->
        let search = ctx.Request.Query["search"].ToString().Trim()
        let statusFilter = ctx.Request.Query["status"].ToString()
        let successMessage = ctx.Request.Query["success"].ToString()
        let errorMessage = ctx.Request.Query["error"].ToString()
        let sort = ctx.Request.Query["sort"].ToString()

        let successHtml =
            if String.IsNullOrWhiteSpace(successMessage) then
                ""
            else
                "<div class=\"message success\">" + successMessage + "</div>"

        let errorHtml =
            if String.IsNullOrWhiteSpace(errorMessage) then
                ""
            else
                "<div class=\"message error\">" + errorMessage + "</div>"

        let statusOption =
            if String.IsNullOrWhiteSpace(statusFilter) then
                None
            else
                parseStatus statusFilter

        let filtered =
            applications
            |> Seq.toList
            |> filterBySearchAndStatus search statusOption

        let sorted =
            match sort with
            | "company" -> filtered |> List.sortBy (fun a -> a.Company)
            | _ -> filtered |> List.sortByDescending (fun a -> a.DateApplied)

        let latestApplication =
            if List.isEmpty(sorted) then
                None
            else
                Some (sorted |> List.maxBy (fun a -> a.DateApplied))

        let latestHtml =
            match latestApplication with
            | Some a ->
                "<div class=\"highlight-box\">" +
                "<b>Latest application:</b> " + a.Company + " - " + a.Position +
                " <span style=\"background:" + statusColor a.Status + ";color:white;padding:4px 8px;border-radius:5px;margin-left:8px;\">" +
                statusToString a.Status +
                "</span></div>"
            | None ->
                ""

        let stats = calculateStatistics sorted

        let totalCount = stats.Total
        let appliedCount = stats.Applied
        let interviewCount = stats.Interview
        let rejectedCount = stats.Rejected

        let rejectionRate =
            if totalCount = 0 then
                "0.0%"
            else
                ((float rejectedCount / float totalCount) * 100.0).ToString("0.0") + "%"

        let favoriteApplications =
            sorted |> List.filter (fun a -> a.IsFavorite)

        let favoritesHtml =
            if List.isEmpty(favoriteApplications) then
                ""
            else
                let items =
                    favoriteApplications
                    |> List.map (fun a ->
                        "<div style=\"padding:10px 0;border-bottom:1px solid #eee;\">" +
                        "⭐ <b>" + a.Company + "</b> - " + a.Position +
                        " <span style=\"color:#666;\">(" + statusToString a.Status + ")</span>" +
                        "</div>"
                    )
                    |> String.concat ""

                "<div class=\"favorites-box\">" +
                "<h2>Favorite Applications</h2>" +
                items +
                "</div>"

        let recentApplications =
            sorted
            |> List.sortByDescending (fun a -> a.DateApplied)
            |> List.truncate 3

        let recentHtml =
            if List.isEmpty(recentApplications) then
                ""
            else
                let items =
                    recentApplications
                    |> List.map (fun a ->
                        "<div class=\"recent-item\">" +
                        "<b>" + a.Company + "</b> - " + a.Position +
                        "<div style=\"font-size:13px;color:#666;margin-top:4px;\">Applied on " + a.DateApplied.ToString("yyyy-MM-dd") + "</div>" +
                        "</div>"
                    )
                    |> String.concat ""

                "<h2 style=\"margin-top:30px;\">Recent Applications</h2>" + items

        let rows =
            sorted
            |> List.map (fun a ->
                let color = statusColor a.Status
                let statusText = statusToString a.Status
                let favoritePrefix =
                    if a.IsFavorite then "⭐ "
                    else ""

                let favoriteActionText =
                    if a.IsFavorite then "Unfavorite"
                    else "Favorite"

                "<tr>" +
                "<td>" + favoritePrefix + a.Company + "</td>" +
                "<td>" + a.Position + "</td>" +
                "<td><span style=\"background:" + color + ";color:white;padding:4px 8px;border-radius:5px;\">" + statusText + "</span></td>" +
                "<td>" + a.DateApplied.ToString("yyyy-MM-dd") + "</td>" +
                "<td>" + a.Notes + "</td>" +
                "<td>" +
                "<a href=\"/application/" + string a.Id + "\">View</a> | " +
                "<a href=\"/edit/" + string a.Id + "\">Edit</a> | " +
                "<a href=\"/delete/" + string a.Id + "\" onclick=\"return confirm('Are you sure?')\">Delete</a> | " +
                "<a href=\"/favorite/" + string a.Id + "\">" + favoriteActionText + "</a>" +
                "</td>" +
                "</tr>"
            )
            |> String.concat ""

        let selectedStatusAll =
            if String.IsNullOrWhiteSpace(statusFilter) then
                "selected"
            else
                ""

        let selectedStatusApplied =
            if statusFilter = "Applied" then
                "selected"
            else
                ""

        let selectedStatusInterview =
            if statusFilter = "Interview" then
                "selected"
            else
                ""

        let selectedStatusRejected =
            if statusFilter = "Rejected" then
                "selected"
            else
                ""

        let selectedDate =
            if String.IsNullOrWhiteSpace(sort) || sort = "date" then
                "selected"
            else
                ""

        let selectedCompany =
            if sort = "company" then
                "selected"
            else
                ""

        let applicationsContent =
            if List.isEmpty(sorted) then
                "<div class=\"empty-message\">No applications found</div>"
            else
                "<table>" +
                "<tr><th>Company</th><th>Position</th><th>Status</th><th>Date</th><th>Notes</th><th>Action</th></tr>" +
                rows +
                "</table>"

        let body =
            latestHtml +
            "<h1>Job Applications</h1>" +
            successHtml +
            errorHtml +
            "<div style=\"text-align:center;margin-bottom:25px;\">" +
            cardHtml "Total" (string totalCount) "#2c3e50" +
            cardHtml "Applied" (string appliedCount) "green" +
            cardHtml "Interview" (string interviewCount) "orange" +
            cardHtml "Rejected" (string rejectedCount) "red" +
            "</div>" +
            "<div style=\"text-align:center;margin-bottom:20px;\">" +
            "<div style=\"display:inline-block;background:white;padding:12px 18px;border-radius:10px;box-shadow:0 2px 8px rgba(0,0,0,0.06);margin:6px;\"><b>Rejection rate:</b> " + rejectionRate + "</div>" +
            "</div>" +
            "<div style=\"text-align:center;margin-bottom:20px;\">" +
            "<a class=\"btn\" href=\"/add-application\">+ Add Application</a> " +
            "<a class=\"btn\" href=\"/stats\">View Statistics</a>" +
            "</div>" +
            "<form method=\"get\" action=\"/applications-page\" style=\"text-align:center;margin-bottom:20px;\">" +
            "<input name=\"search\" value=\"" + search + "\" placeholder=\"Search\" /> " +
            "<select name=\"status\">" +
            "<option value=\"\" " + selectedStatusAll + ">All</option>" +
            "<option value=\"Applied\" " + selectedStatusApplied + ">Applied</option>" +
            "<option value=\"Interview\" " + selectedStatusInterview + ">Interview</option>" +
            "<option value=\"Rejected\" " + selectedStatusRejected + ">Rejected</option>" +
            "</select> " +
            "<select name=\"sort\">" +
            "<option value=\"date\" " + selectedDate + ">Date</option>" +
            "<option value=\"company\" " + selectedCompany + ">Company</option>" +
            "</select> " +
            "<button class=\"btn\" type=\"submit\">Filter</button> " +
            "<a class=\"btn\" href=\"/applications-page\">Clear</a>" +
            "</form>" +
            applicationsContent +
            favoritesHtml +
            recentHtml

        htmlPage "Applications" body
    )) |> ignore

    app.MapGet("/application/{id}", Func<int, IResult>(fun id ->
        let item =
            applications |> Seq.tryFind (fun a -> a.Id = id)

        match item with
        | Some a ->
            let color = statusColor a.Status
            let statusText = statusToString a.Status
            let favoriteText =
                if a.IsFavorite then "Yes"
                else "No"

            let body =
                "<h1>Application Details</h1>" +
                "<p><b>Company:</b> " + a.Company + "</p>" +
                "<p><b>Position:</b> " + a.Position + "</p>" +
                "<p><b>Status:</b> <span style=\"background:" + color + ";color:white;padding:4px 8px;border-radius:5px;\">" + statusText + "</span></p>" +
                "<p><b>Date applied:</b> " + a.DateApplied.ToString("yyyy-MM-dd") + "</p>" +
                "<p><b>Notes:</b> " + a.Notes + "</p>" +
                "<p><b>Favorite:</b> " + favoriteText + "</p>" +
                "<p><a href=\"/applications-page\">Back to list</a></p>"

            htmlPage "Details" body
        | None ->
            htmlPage "Not found" "<h1>Application not found</h1><p><a href=\"/applications-page\">Back to list</a></p>"
    )) |> ignore

    app.MapGet("/add-application", Func<HttpContext, IResult>(fun ctx ->
        let errorMessage = ctx.Request.Query["error"].ToString()

        let errorHtml =
            if String.IsNullOrWhiteSpace(errorMessage) then
                ""
            else
                "<div class=\"message error\">" + errorMessage + "</div>"

        let body =
            "<h1>Add Application</h1>" +
            errorHtml +
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
            if req.Form.ContainsKey(name) then req.Form[name].ToString().Trim()
            else ""

        let company = get "company"
        let position = get "position"
        let status = get "status"
        let notes = get "notes"

        match validateApplication company position status with
        | Some error ->
            Results.Redirect("/add-application?error=" + Uri.EscapeDataString(error))
        | None ->
            let statusValue =
                parseStatus status |> Option.defaultValue Applied

            let newApp =
                {
                    Id = getNextId ()
                    Company = company
                    Position = position
                    DateApplied = DateTime.Now
                    Status = statusValue
                    Notes = notes
                    IsFavorite = false
                }

            applications.Add(newApp)
            Results.Redirect("/applications-page?success=Application added successfully")
    )) |> ignore

    app.MapGet("/edit/{id}", Func<HttpContext, int, IResult>(fun ctx id ->
        let errorMessage = ctx.Request.Query["error"].ToString()

        let item =
            applications |> Seq.tryFind (fun a -> a.Id = id)

        match item with
        | Some a ->
            let selectedApplied = if a.Status = Applied then "selected" else ""
            let selectedInterview = if a.Status = Interview then "selected" else ""
            let selectedRejected = if a.Status = Rejected then "selected" else ""

            let errorHtml =
                if String.IsNullOrWhiteSpace(errorMessage) then
                    ""
                else
                    "<div class=\"message error\">" + errorMessage + "</div>"

            let body =
                "<h1>Edit Application</h1>" +
                errorHtml +
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
            if req.Form.ContainsKey(name) then req.Form[name].ToString().Trim()
            else fallback

        let existing =
            applications |> Seq.tryFind (fun a -> a.Id = id)

        match existing with
        | Some oldItem ->
            let company = get "company" oldItem.Company
            let position = get "position" oldItem.Position
            let status = get "status" (statusToString oldItem.Status)
            let notes = get "notes" oldItem.Notes

            match validateApplication company position status with
            | Some error ->
                Results.Redirect("/edit/" + string id + "?error=" + Uri.EscapeDataString(error))
            | None ->
                let statusValue =
                    parseStatus status |> Option.defaultValue oldItem.Status

                let index = applications |> Seq.findIndex (fun a -> a.Id = id)

                applications.[index] <-
                    {
                        Id = oldItem.Id
                        Company = company
                        Position = position
                        DateApplied = oldItem.DateApplied
                        Status = statusValue
                        Notes = notes
                        IsFavorite = oldItem.IsFavorite
                    }

                Results.Redirect("/applications-page?success=Application updated successfully")
        | None ->
            htmlPage "Not found" "<h1>Application not found</h1><p><a href=\"/applications-page\">Back to list</a></p>"
    )) |> ignore

    app.MapGet("/delete/{id}", Func<int, IResult>(fun id ->
        applications.RemoveAll(fun a -> a.Id = id) |> ignore
        Results.Redirect("/applications-page?success=Application deleted successfully")
    )) |> ignore

    app.MapGet("/stats", Func<IResult>(fun () ->
        let stats =
            applications
            |> Seq.toList
            |> calculateStatistics

        let total = stats.Total
        let applied = stats.Applied
        let interview = stats.Interview
        let rejected = stats.Rejected

        let appliedPercent = percentage applied total
        let interviewPercent = percentage interview total
        let rejectedPercent = percentage rejected total

        let latest =
            if applications.Count = 0 then
                "N/A"
            else
                let latestApp =
                    applications
                    |> Seq.maxBy (fun a -> a.DateApplied)

                latestApp.DateApplied.ToString("yyyy-MM-dd")

        let mostCommonStatus =
            if applications.Count = 0 then
                "N/A"
            else
                applications
                |> Seq.groupBy (fun a -> a.Status)
                |> Seq.maxBy (fun (_, apps) -> Seq.length apps)
                |> fun (status, _) -> statusToString status

        let favoriteCount =
            applications
            |> Seq.filter (fun a -> a.IsFavorite)
            |> Seq.length

        let body =
            "<h1>Statistics</h1>" +
            "<div class=\"stats\">" +

            "<p><b>Total applications:</b> " + string total + "</p>" +

            "<p><b>Applied:</b> " + string applied + " (" + appliedPercent.ToString("0.0") + "%)</p>" +
            "<div style=\"width:300px;margin:0 auto 15px auto;background:#ddd;border-radius:8px;overflow:hidden;\">" +
            "<div class=\"progress-bar\" style=\"width:" + appliedPercent.ToString("0.0", Globalization.CultureInfo.InvariantCulture) + "%;background:green;\"></div>" +
            "</div>" +

            "<p><b>Interview:</b> " + string interview + " (" + interviewPercent.ToString("0.0") + "%)</p>" +
            "<div style=\"width:300px;margin:0 auto 15px auto;background:#ddd;border-radius:8px;overflow:hidden;\">" +
            "<div class=\"progress-bar\" style=\"width:" + interviewPercent.ToString("0.0", Globalization.CultureInfo.InvariantCulture) + "%;background:orange;\"></div>" +
            "</div>" +

            "<p><b>Rejected:</b> " + string rejected + " (" + rejectedPercent.ToString("0.0") + "%)</p>" +
            "<div style=\"width:300px;margin:0 auto 15px auto;background:#ddd;border-radius:8px;overflow:hidden;\">" +
            "<div class=\"progress-bar\" style=\"width:" + rejectedPercent.ToString("0.0", Globalization.CultureInfo.InvariantCulture) + "%;background:red;\"></div>" +
            "</div>" +

            "<p><b>Latest application:</b> " + latest + "</p>" +
            "<p><b>Most common status:</b> " + mostCommonStatus + "</p>" +
            "<p><b>Favorite applications:</b> " + string favoriteCount + "</p>" +
            "</div>" +

            "<p style=\"text-align:center;\">" +
            "<a class=\"btn\" href=\"/applications-page\">Back to applications</a>" +
            "</p>"

        htmlPage "Statistics" body
    )) |> ignore

    app.Run()
    0