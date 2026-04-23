module Domain

open System

type ApplicationStatus =
    | Applied
    | Interview
    | Rejected

type Priority =
    | Low
    | Medium
    | High

type Application =
    {
        Id: int
        Company: string
        Position: string
        DateApplied: DateTime
        Status: ApplicationStatus
        Notes: string
        IsFavorite: bool
        FollowUpDate: DateTime option
        Priority: Priority
    }

type State =
    {
        Applications: Application list
        SearchText: string
    }

let initialState =
    {
        Applications =
            [
                {
                    Id = 1
                    Company = "Microsoft"
                    Position = "Backend Intern"
                    DateApplied = DateTime(2026, 3, 1)
                    Status = Applied
                    Notes = "Applied through website"
                    IsFavorite = false
                    FollowUpDate = Some(DateTime(2026, 4, 25))
                    Priority = High
                }
                {
                    Id = 2
                    Company = "Google"
                    Position = "Software Engineer Intern"
                    DateApplied = DateTime(2026, 3, 3)
                    Status = Interview
                    Notes = "HR round done"
                    IsFavorite = true
                    FollowUpDate = Some(DateTime(2026, 4, 23))
                    Priority = High
                }
                {
                    Id = 3
                    Company = "SAP"
                    Position = "Junior Developer"
                    DateApplied = DateTime(2026, 3, 5)
                    Status = Rejected
                    Notes = "Rejected email"
                    IsFavorite = false
                    FollowUpDate = None
                    Priority = Low
                }
            ]
        SearchText = ""
    }

let containsInsensitive (needle: string) (haystack: string) =
    haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0

let matchesSearch (search: string) (app: Application) =
    if String.IsNullOrWhiteSpace(search) then
        true
    else
        containsInsensitive search app.Company
        || containsInsensitive search app.Position
        || containsInsensitive search app.Notes

let filterApplications (state: State) =
    state.Applications
    |> List.filter (matchesSearch state.SearchText)

let matchesStatus (statusFilter: ApplicationStatus option) (app: Application) =
    match statusFilter with
    | None -> true
    | Some s -> app.Status = s

let matchesPriority (priorityFilter: Priority option) (app: Application) =
    match priorityFilter with
    | None -> true
    | Some p -> app.Priority = p

let filterBySearchAndStatus (search: string) (status: ApplicationStatus option) (apps: Application list) =
    apps
    |> List.filter (matchesSearch search)
    |> List.filter (matchesStatus status)

let filterBySearchStatusAndPriority
    (search: string)
    (status: ApplicationStatus option)
    (priority: Priority option)
    (apps: Application list) =
    apps
    |> List.filter (matchesSearch search)
    |> List.filter (matchesStatus status)
    |> List.filter (matchesPriority priority)

type Statistics =
    {
        Total: int
        Applied: int
        Interview: int
        Rejected: int
        Favorites: int
        HighPriority: int
    }

let emptyStatistics =
    {
        Total = 0
        Applied = 0
        Interview = 0
        Rejected = 0
        Favorites = 0
        HighPriority = 0
    }

let calculateStatistics (apps: Application list) =
    apps
    |> List.fold (fun acc app ->
        let updatedAcc =
            match app.Status with
            | Applied ->
                { acc with
                    Total = acc.Total + 1
                    Applied = acc.Applied + 1 }
            | Interview ->
                { acc with
                    Total = acc.Total + 1
                    Interview = acc.Interview + 1 }
            | Rejected ->
                { acc with
                    Total = acc.Total + 1
                    Rejected = acc.Rejected + 1 }

        {
            updatedAcc with
                Favorites = updatedAcc.Favorites + (if app.IsFavorite then 1 else 0)
                HighPriority = updatedAcc.HighPriority + (if app.Priority = High then 1 else 0)
        }
    ) emptyStatistics

let parseStatus (status: string) =
    match status.Trim().ToLowerInvariant() with
    | "applied" -> Some Applied
    | "interview" -> Some Interview
    | "rejected" -> Some Rejected
    | _ -> None

let parsePriority (priority: string) =
    match priority.Trim().ToLowerInvariant() with
    | "low" -> Some Low
    | "medium" -> Some Medium
    | "high" -> Some High
    | _ -> None

let priorityToString priority =
    match priority with
    | Low -> "Low"
    | Medium -> "Medium"
    | High -> "High"

let validateApplication company position status priority =
    if String.IsNullOrWhiteSpace(company) then
        Some "Company is required."
    elif String.IsNullOrWhiteSpace(position) then
        Some "Position is required."
    elif Option.isNone (parseStatus status) then
        Some "Invalid status selected."
    elif Option.isNone (parsePriority priority) then
        Some "Invalid priority selected."
    else
        None

let percentage part total =
    if total = 0 then
        0.0
    else
        (float part / float total) * 100.0

let tryParseDate (value: string) =
    match DateTime.TryParse(value) with
    | true, date -> Some date
    | _ -> None