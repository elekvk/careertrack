open System
open System.Net
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
            "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">" +
            "<title>" + title + "</title>" +
            "<style>" +
            "body { font-family: Arial; background-color: #f4f6f8; margin:0; padding:0; color:#2c3e50; }" +
            "body, input, select, textarea, button { font-family: Arial, sans-serif; }" +
            "h1 { text-align:center; color:#2c3e50; margin-bottom:8px; }" +
            "h2 { color:#2c3e50; }" +
            ".container { width: 85%; max-width: 1200px; margin: auto; padding: 24px; }" +
            "table { width: 100%; border-collapse: collapse; background:white; box-shadow:0 2px 8px rgba(0,0,0,0.1); border-radius:12px; overflow:hidden; }" +
            "th, td { padding: 12px; border-bottom: 1px solid #eee; text-align:left; vertical-align:top; }" +
            "th { background-color: #2c3e50; color:white; }" +
            "tr:hover { background-color: #f9fbfc; }" +
            "a { text-decoration:none; color:#3498db; font-weight:bold; }" +
            "a:hover { text-decoration:underline; }" +
            ".btn { display:inline-block; padding:10px 15px; background:#3498db; color:white; border-radius:8px; border:none; cursor:pointer; margin:4px; font-weight:bold; }" +
            ".btn:hover { background:#2980b9; text-decoration:none; }" +
            ".btn-secondary { background:#7f8c8d; }" +
            ".btn-secondary:hover { background:#6c7a7a; }" +
            "input, select, textarea { padding:10px; width:320px; max-width:100%; margin-bottom:10px; border:1px solid #dcdfe3; border-radius:8px; box-sizing:border-box; }" +
            "textarea { min-height:100px; resize:vertical; }" +
            ".stats { text-align:center; margin-bottom:20px; font-size:18px; }" +
            ".message { text-align:center; font-weight:bold; padding:12px; border-radius:8px; margin-bottom:20px; }" +
            ".success { color:green; background:#eafaf1; }" +
            ".error { color:#b00020; background:#fdecea; }" +
            ".empty-message { text-align:center; background:white; padding:24px; border-radius:12px; box-shadow:0 2px 8px rgba(0,0,0,0.1); font-weight:bold; color:#555; margin-top:20px; }" +
            ".highlight-box { background:#e8f5e9; padding:16px; border-radius:12px; margin-bottom:20px; text-align:center; box-shadow:0 2px 8px rgba(0,0,0,0.08); }" +
            ".progress-bar { height:20px; border-radius:8px; transition: width 0.8s ease-in-out; }" +
            ".card { display:inline-block; width:180px; margin:10px; padding:20px; background:white; border-radius:12px; box-shadow:0 2px 8px rgba(0,0,0,0.08); text-align:center; vertical-align:top; }" +
            ".card-title { font-size:14px; color:#666; margin-bottom:8px; }" +
            ".card-value { font-size:28px; font-weight:bold; }" +
            ".favorites-box { background:#fff8e1; padding:16px; border-radius:12px; margin-top:20px; box-shadow:0 2px 8px rgba(0,0,0,0.06); }" +
            ".recent-item { background:white; padding:12px 16px; margin-bottom:10px; border-radius:10px; box-shadow:0 2px 8px rgba(0,0,0,0.06); }" +
            ".subtitle { text-align:center; font-size:18px; color:#555; margin-top:-8px; margin-bottom:24px; }" +
            ".badge { color:white; padding:4px 8px; border-radius:6px; font-size:13px; font-weight:bold; display:inline-block; }" +
            ".section-box { background:white; padding:18px; border-radius:12px; box-shadow:0 2px 8px rgba(0,0,0,0.06); margin-top:20px; }" +
            ".muted { color:#666; font-size:14px; }" +
            ".form-box { max-width:420px; margin:0 auto; background:white; padding:24px; border-radius:12px; box-shadow:0 2px 8px rgba(0,0,0,0.08); }" +
            ".two-column { display:grid; grid-template-columns: 1fr 1fr; gap:20px; margin-top:20px; }" +
            "@media (max-width: 900px) { .two-column { grid-template-columns: 1fr; } .container { width:92%; } }" +
            "</style>" +
            "</head>" +
            "<body><div class=\"container\">" +
            body +
            "</div></body></html>",
            "text/html; charset=utf-8"
        )

    let esc (s: string) =
        WebUtility.HtmlEncode(s)

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

    let priorityToColor priority =
        match priority with
        | Low -> "#6c757d"
        | Medium -> "#f39c12"
        | High -> "#c0392b"

    let formatFollowUpDate (dateOpt: DateTime option) =
        match dateOpt with
        | Some d -> d.ToString("yyyy-MM-dd")
        | None -> "-"

    let cardHtml title value color =
        "<div class=\"card\">" +
        "<div class=\"card-title\">" + esc title + "</div>" +
        "<div class=\"card-value\" style=\"color:" + color + ";\">" + esc value + "</div>" +
        "</div>"

    let applications = ResizeArray<Application>(initialState.Applications)

    let getNextId () =
        if applications.Count = 0 then
            1
        else
            applications
            |> Seq.map (fun a -> a.Id)
            |> Seq.max
            |> fun maxId -> maxId + 1

    app.MapGet("/", Func<IResult>(fun () ->
        let body =
            "<h1>CareerTrack</h1>" +
            "<div class=\"subtitle\">Track your job and internship applications in one place</div>" +
            "<p style=\"text-align:center;color:#666;\">Built with F# and ASP.NET Core</p>" +
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
        let priorityFilter = ctx.Request.Query["priority"].ToString()
        let successMessage = ctx.Request.Query["success"].ToString()
        let errorMessage = ctx.Request.Query["error"].ToString()
        let sort = ctx.Request.Query["sort"].ToString()

        let successHtml =
            if String.IsNullOrWhiteSpace(successMessage) then
                ""
            else
                "<div class=\"message success\">" + esc successMessage + "</div>"

        let errorHtml =
            if String.IsNullOrWhiteSpace(errorMessage) then
                ""
            else
                "<div class=\"message error\">" + esc errorMessage + "</div>"

        let statusOption =
            if String.IsNullOrWhiteSpace(statusFilter) then
                None
            else
                parseStatus statusFilter

        let priorityOption =
            if String.IsNullOrWhiteSpace(priorityFilter) then
                None
            else
                parsePriority priorityFilter

        let filtered =
            applications
            |> Seq.toList
            |> filterBySearchStatusAndPriority search statusOption priorityOption

        let sorted =
            match sort with
            | "company" -> filtered |> List.sortBy (fun a -> a.Company)
            | "priority" ->
                filtered
                |> List.sortByDescending (fun a ->
                    match a.Priority with
                    | High -> 3
                    | Medium -> 2
                    | Low -> 1)
            | _ ->
                filtered |> List.sortByDescending (fun a -> a.DateApplied)

        let latestApplication =
            if List.isEmpty(sorted) then
                None
            else
                Some (sorted |> List.maxBy (fun a -> a.DateApplied))

        let latestHtml =
            match latestApplication with
            | Some a ->
                "<div class=\"highlight-box\">" +
                "<b>Latest application:</b> " + esc a.Company + " - " + esc a.Position +
                " <span class=\"badge\" style=\"background:" + statusColor a.Status + "; margin-left:8px;\">" +
                statusToString a.Status +
                "</span></div>"
            | None ->
                ""

        let stats = calculateStatistics sorted

        let totalCount = stats.Total
        let appliedCount = stats.Applied
        let interviewCount = stats.Interview
        let rejectedCount = stats.Rejected
        let favoriteCount = stats.Favorites
        let highPriorityCount = stats.HighPriority

        let rejectionRate =
            if totalCount = 0 then
                "0.0%"
            else
                ((float rejectedCount / float totalCount) * 100.0).ToString("0.0") + "%"

        let favoriteApplications =
            sorted |> List.filter (fun a -> a.IsFavorite)

        let favoritesHtml =
            if List.isEmpty(favoriteApplications) then
                "<div class=\"section-box\"><h2>Favorite Applications</h2><div class=\"muted\">No favorite applications yet.</div></div>"
            else
                let items =
                    favoriteApplications
                    |> List.map (fun a ->
                        "<div style=\"padding:10px 0;border-bottom:1px solid #eee;\">" +
                        "⭐ <b>" + esc a.Company + "</b> - " + esc a.Position +
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
                        "<b>" + esc a.Company + "</b> - " + esc a.Position +
                        "<div style=\"font-size:13px;color:#666;margin-top:4px;\">Applied on " + a.DateApplied.ToString("yyyy-MM-dd") + "</div>" +
                        "</div>"
                    )
                    |> String.concat ""

                "<h2 style=\"margin-top:30px;\">Recent Applications</h2>" + items

        let upcomingActions =
            applications
            |> Seq.toList
            |> List.choose (fun a ->
                match a.FollowUpDate with
                | Some d -> Some (a, d)
                | None -> None)
            |> List.sortBy snd
            |> List.truncate 5

        let upcomingActionsHtml =
            if List.isEmpty(upcomingActions) then
                "<div class=\"section-box\"><h2>Upcoming Actions</h2><div class=\"muted\">No follow-up dates set yet.</div></div>"
            else
                let today = DateTime.Today

                let items =
                    upcomingActions
                    |> List.map (fun (a, d) ->
                        let overdue =
                            if d.Date < today then
                                "<span class=\"badge\" style=\"background:#c0392b; margin-left:8px;\">Overdue</span>"
                            else
                                ""

                        "<div style=\"padding:10px 0;border-bottom:1px solid #eee;\">" +
                        "<b>" + esc a.Company + "</b> - " + esc a.Position +
                        "<div class=\"muted\" style=\"margin-top:4px;\">Follow-up: " + d.ToString("yyyy-MM-dd") + overdue + "</div>" +
                        "</div>"
                    )
                    |> String.concat ""

                "<div class=\"section-box\"><h2>Upcoming Actions</h2>" + items + "</div>"

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
                let priorityText = priorityToString a.Priority
                let priorityColor = priorityToColor a.Priority
                let followUpText = formatFollowUpDate a.FollowUpDate

                "<tr>" +
                "<td>" + favoritePrefix + esc a.Company + "</td>" +
                "<td>" + esc a.Position + "</td>" +
                "<td><span class=\"badge\" style=\"background:" + color + ";\">" + statusText + "</span></td>" +
                "<td><span class=\"badge\" style=\"background:" + priorityColor + ";\">" + priorityText + "</span></td>" +
                "<td>" + esc followUpText + "</td>" +
                "<td>" + a.DateApplied.ToString("yyyy-MM-dd") + "</td>" +
                "<td>" + esc a.Notes + "</td>" +
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
            if String.IsNullOrWhiteSpace(statusFilter) then "selected" else ""
        let selectedStatusApplied =
            if statusFilter = "Applied" then "selected" else ""
        let selectedStatusInterview =
            if statusFilter = "Interview" then "selected" else ""
        let selectedStatusRejected =
            if statusFilter = "Rejected" then "selected" else ""

        let selectedPriorityAll =
            if String.IsNullOrWhiteSpace(priorityFilter) then "selected" else ""
        let selectedPriorityLow =
            if priorityFilter = "Low" then "selected" else ""
        let selectedPriorityMedium =
            if priorityFilter = "Medium" then "selected" else ""
        let selectedPriorityHigh =
            if priorityFilter = "High" then "selected" else ""

        let selectedDate =
            if String.IsNullOrWhiteSpace(sort) || sort = "date" then "selected" else ""
        let selectedCompany =
            if sort = "company" then "selected" else ""
        let selectedPrioritySort =
            if sort = "priority" then "selected" else ""

        let applicationsContent =
            if List.isEmpty(sorted) then
                "<div class=\"empty-message\">No applications found</div>"
            else
                "<table>" +
                "<tr><th>Company</th><th>Position</th><th>Status</th><th>Priority</th><th>Follow-up</th><th>Date</th><th>Notes</th><th>Action</th></tr>" +
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
            cardHtml "Favorites" (string favoriteCount) "#f1c40f" +
            cardHtml "High Priority" (string highPriorityCount) "#c0392b" +
            "</div>" +
            "<div style=\"text-align:center;margin-bottom:20px;\">" +
            "<div style=\"display:inline-block;background:white;padding:12px 18px;border-radius:10px;box-shadow:0 2px 8px rgba(0,0,0,0.06);margin:6px;\"><b>Rejection rate:</b> " + rejectionRate + "</div>" +
            "</div>" +
            "<div style=\"text-align:center;margin-bottom:20px;\">" +
            "<a class=\"btn\" href=\"/add-application\">+ Add Application</a> " +
            "<a class=\"btn\" href=\"/stats\">View Statistics</a>" +
            "</div>" +
            "<form method=\"get\" action=\"/applications-page\" style=\"text-align:center;margin-bottom:20px;\">" +
            "<input name=\"search\" value=\"" + esc search + "\" placeholder=\"Search by company or position\" /> " +
            "<select name=\"status\">" +
            "<option value=\"\" " + selectedStatusAll + ">All Statuses</option>" +
            "<option value=\"Applied\" " + selectedStatusApplied + ">Applied</option>" +
            "<option value=\"Interview\" " + selectedStatusInterview + ">Interview</option>" +
            "<option value=\"Rejected\" " + selectedStatusRejected + ">Rejected</option>" +
            "</select> " +
            "<select name=\"priority\">" +
            "<option value=\"\" " + selectedPriorityAll + ">All Priorities</option>" +
            "<option value=\"Low\" " + selectedPriorityLow + ">Low</option>" +
            "<option value=\"Medium\" " + selectedPriorityMedium + ">Medium</option>" +
            "<option value=\"High\" " + selectedPriorityHigh + ">High</option>" +
            "</select> " +
            "<select name=\"sort\">" +
            "<option value=\"date\" " + selectedDate + ">Date</option>" +
            "<option value=\"company\" " + selectedCompany + ">Company</option>" +
            "<option value=\"priority\" " + selectedPrioritySort + ">Priority</option>" +
            "</select> " +
            "<button class=\"btn\" type=\"submit\">Filter</button> " +
            "<a class=\"btn btn-secondary\" href=\"/applications-page\">Clear</a>" +
            "</form>" +
            applicationsContent +
            "<div class=\"two-column\">" +
            upcomingActionsHtml +
            favoritesHtml +
            "</div>" +
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
                "<div class=\"form-box\" style=\"max-width:700px;\">" +
                "<p><b>Company:</b> " + esc a.Company + "</p>" +
                "<p><b>Position:</b> " + esc a.Position + "</p>" +
                "<p><b>Status:</b> <span class=\"badge\" style=\"background:" + color + ";\">" + statusText + "</span></p>" +
                "<p><b>Priority:</b> <span class=\"badge\" style=\"background:" + priorityToColor a.Priority + ";\">" + priorityToString a.Priority + "</span></p>" +
                "<p><b>Date applied:</b> " + a.DateApplied.ToString("yyyy-MM-dd") + "</p>" +
                "<p><b>Follow-up date:</b> " + esc (formatFollowUpDate a.FollowUpDate) + "</p>" +
                "<p><b>Notes:</b> " + esc a.Notes + "</p>" +
                "<p><b>Favorite:</b> " + favoriteText + "</p>" +
                "<p><a href=\"/applications-page\">Back to list</a></p>" +
                "</div>"

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
                "<div class=\"message error\">" + esc errorMessage + "</div>"

        let body =
            "<h1>Add Application</h1>" +
            errorHtml +
            "<div class=\"form-box\">" +
            "<form method=\"post\" action=\"/applications\">" +
            "<input name=\"company\" placeholder=\"Company\" /><br/>" +
            "<input name=\"position\" placeholder=\"Position\" /><br/>" +
            "<select name=\"status\">" +
            "<option value=\"Applied\">Applied</option>" +
            "<option value=\"Interview\">Interview</option>" +
            "<option value=\"Rejected\">Rejected</option>" +
            "</select><br/>" +
            "<select name=\"priority\">" +
            "<option value=\"Low\">Low</option>" +
            "<option value=\"Medium\" selected>Medium</option>" +
            "<option value=\"High\">High</option>" +
            "</select><br/>" +
            "<input type=\"date\" name=\"followUpDate\" /><br/>" +
            "<textarea name=\"notes\" placeholder=\"Notes\"></textarea><br/>" +
            "<button class=\"btn\" type=\"submit\">Save</button>" +
            "</form>" +
            "<p><a href=\"/applications-page\">Back to list</a></p>" +
            "</div>"

        htmlPage "Add Application" body
    )) |> ignore

    app.MapPost("/applications", Func<HttpRequest, IResult>(fun req ->
        let get name =
            if req.Form.ContainsKey(name) then req.Form[name].ToString().Trim()
            else ""

        let company = get "company"
        let position = get "position"
        let status = get "status"
        let priority = get "priority"
        let followUpDateRaw = get "followUpDate"
        let notes = get "notes"

        match validateApplication company position status priority with
        | Some error ->
            Results.Redirect("/add-application?error=" + Uri.EscapeDataString(error))
        | None ->
            let statusValue =
                parseStatus status |> Option.defaultValue Applied

            let priorityValue =
                parsePriority priority |> Option.defaultValue Medium

            let followUpDate =
                if String.IsNullOrWhiteSpace(followUpDateRaw) then
                    None
                else
                    tryParseDate followUpDateRaw

            let newApp =
                {
                    Id = getNextId ()
                    Company = company
                    Position = position
                    DateApplied = DateTime.Now
                    Status = statusValue
                    Notes = notes
                    IsFavorite = false
                    FollowUpDate = followUpDate
                    Priority = priorityValue
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

            let selectedLow = if a.Priority = Low then "selected" else ""
            let selectedMedium = if a.Priority = Medium then "selected" else ""
            let selectedHigh = if a.Priority = High then "selected" else ""

            let followUpValue =
                match a.FollowUpDate with
                | Some d -> d.ToString("yyyy-MM-dd")
                | None -> ""

            let errorHtml =
                if String.IsNullOrWhiteSpace(errorMessage) then
                    ""
                else
                    "<div class=\"message error\">" + esc errorMessage + "</div>"

            let body =
                "<h1>Edit Application</h1>" +
                errorHtml +
                "<div class=\"form-box\">" +
                "<form method=\"post\" action=\"/update/" + string a.Id + "\">" +
                "<input name=\"company\" value=\"" + esc a.Company + "\" /><br/>" +
                "<input name=\"position\" value=\"" + esc a.Position + "\" /><br/>" +
                "<select name=\"status\">" +
                "<option value=\"Applied\" " + selectedApplied + ">Applied</option>" +
                "<option value=\"Interview\" " + selectedInterview + ">Interview</option>" +
                "<option value=\"Rejected\" " + selectedRejected + ">Rejected</option>" +
                "</select><br/>" +
                "<select name=\"priority\">" +
                "<option value=\"Low\" " + selectedLow + ">Low</option>" +
                "<option value=\"Medium\" " + selectedMedium + ">Medium</option>" +
                "<option value=\"High\" " + selectedHigh + ">High</option>" +
                "</select><br/>" +
                "<input type=\"date\" name=\"followUpDate\" value=\"" + esc followUpValue + "\" /><br/>" +
                "<textarea name=\"notes\">" + esc a.Notes + "</textarea><br/>" +
                "<button class=\"btn\" type=\"submit\">Update</button>" +
                "</form>" +
                "<p><a href=\"/applications-page\">Back to list</a></p>" +
                "</div>"

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
            let priority = get "priority" (priorityToString oldItem.Priority)
            let followUpDateRaw = get "followUpDate" (formatFollowUpDate oldItem.FollowUpDate)
            let notes = get "notes" oldItem.Notes

            match validateApplication company position status priority with
            | Some error ->
                Results.Redirect("/edit/" + string id + "?error=" + Uri.EscapeDataString(error))
            | None ->
                let statusValue =
                    parseStatus status |> Option.defaultValue oldItem.Status

                let priorityValue =
                    parsePriority priority |> Option.defaultValue oldItem.Priority

                let followUpDate =
                    if String.IsNullOrWhiteSpace(followUpDateRaw) || followUpDateRaw = "-" then
                        None
                    else
                        tryParseDate followUpDateRaw

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
                        FollowUpDate = followUpDate
                        Priority = priorityValue
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
        let favorites = stats.Favorites
        let highPriority = stats.HighPriority

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

        let body =
            "<h1>Statistics</h1>" +

            "<div style=\"text-align:center;margin-bottom:25px;\">" +
            cardHtml "Total" (string total) "#2c3e50" +
            cardHtml "Applied" (string applied) "green" +
            cardHtml "Interview" (string interview) "orange" +
            cardHtml "Rejected" (string rejected) "red" +
            cardHtml "Favorites" (string favorites) "#f1c40f" +
            cardHtml "High Priority" (string highPriority) "#c0392b" +
            "</div>" +

            "<div class=\"two-column\">" +

            "<div class=\"section-box stats\">" +
            "<h2>Status Overview</h2>" +

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
            "<p><b>Favorite applications:</b> " + string favorites + "</p>" +
            "<p><b>High priority applications:</b> " + string highPriority + "</p>" +
            "</div>" +

            "<div class=\"section-box\" style=\"text-align:center;\">" +
            "<h2>Status Distribution</h2>" +
            "<canvas id=\"statusChart\" width=\"320\" height=\"320\"></canvas>" +
            "</div>" +

            "</div>" +

            "<div class=\"section-box\" style=\"margin-top:20px; text-align:center;\">" +
            "<h2>Applications by Status</h2>" +
            "<canvas id=\"statusBarChart\" height=\"120\"></canvas>" +
            "</div>" +

            "<script src=\"https://cdn.jsdelivr.net/npm/chart.js\"></script>" +
            "<script>" +
            "const doughnutCtx = document.getElementById('statusChart');" +
            "new Chart(doughnutCtx, {" +
            "type: 'doughnut'," +
            "data: {" +
            "labels: ['Applied', 'Interview', 'Rejected']," +
            "datasets: [{" +
            "label: 'Applications'," +
            "data: [" + string applied + ", " + string interview + ", " + string rejected + "]," +
            "backgroundColor: ['#2ecc71', '#f39c12', '#e74c3c']," +
            "borderWidth: 1" +
            "}]" +
            "}," +
            "options: {" +
            "responsive: true," +
            "plugins: {" +
            "legend: { position: 'bottom' }" +
            "}" +
            "}" +
            "});" +

            "const barCtx = document.getElementById('statusBarChart');" +
            "new Chart(barCtx, {" +
            "type: 'bar'," +
            "data: {" +
            "labels: ['Applied', 'Interview', 'Rejected']," +
            "datasets: [{" +
            "label: 'Count'," +
            "data: [" + string applied + ", " + string interview + ", " + string rejected + "]," +
            "backgroundColor: ['#2ecc71', '#f39c12', '#e74c3c']" +
            "}]" +
            "}," +
            "options: {" +
            "responsive: true," +
            "scales: {" +
            "y: {" +
            "beginAtZero: true," +
            "ticks: { precision: 0 }" +
            "}" +
            "}," +
            "plugins: {" +
            "legend: { display: false }" +
            "}" +
            "}" +
            "});" +
            "</script>" +

            "<p style=\"text-align:center; margin-top:20px;\">" +
            "<a class=\"btn\" href=\"/applications-page\">Back to applications</a>" +
            "</p>"

        htmlPage "Statistics" body
    )) |> ignore

    app.Run()
    0