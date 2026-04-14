module Domain

open System

type ApplicationStatus =
    | Applied
    | Interview
    | Rejected

type Application =
    {
        Id: int
        Company: string
        Position: string
        DateApplied: System.DateTime
        Status: ApplicationStatus
        Notes: string
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
                    DateApplied = System.DateTime(2026, 3, 1)
                    Status = Applied
                    Notes = "Applied through website"
                }
                {
                    Id = 2
                    Company = "Google"
                    Position = "Software Engineer Intern"
                    DateApplied = System.DateTime(2026, 3, 3)
                    Status = Interview
                    Notes = "HR round done"
                }
                {
                    Id = 3
                    Company = "SAP"
                    Position = "Junior Developer"
                    DateApplied = System.DateTime(2026, 3, 5)
                    Status = Rejected
                    Notes = "Rejected email"
                }
            ]
        SearchText = ""
    }

let matchesSearch (search: string) (app: Application) =
    let s = search.ToLower()
    app.Company.ToLower().Contains(s)
    || app.Position.ToLower().Contains(s)
    || app.Notes.ToLower().Contains(s)

let filterApplications (state: State) =
    state.Applications
    |> List.filter (matchesSearch state.SearchText)

let matchesStatus (statusFilter: ApplicationStatus option) (app: Application) =
    match statusFilter with
    | None -> true
    | Some s -> app.Status = s

let filterBySearchAndStatus (search: string) (status: ApplicationStatus option) (apps: Application list) =
    apps
    |> List.filter (matchesSearch search)
    |> List.filter (matchesStatus status)

type Statistics =
    {
        Total: int
        Applied: int
        Interview: int
        Rejected: int
    }

let emptyStatistics =
    {
        Total = 0
        Applied = 0
        Interview = 0
        Rejected = 0
    }

let calculateStatistics (apps: Application list) =
    apps
    |> List.fold (fun acc app ->
        match app.Status with
        | Applied ->
            {
                acc with
                    Total = acc.Total + 1
                    Applied = acc.Applied + 1
            }
        | Interview ->
            {
                acc with
                    Total = acc.Total + 1
                    Interview = acc.Interview + 1
            }
        | Rejected ->
            {
                acc with
                    Total = acc.Total + 1
                    Rejected = acc.Rejected + 1
            }
    ) emptyStatistics
let parseStatus (status: string) =
    match status with
    | "Applied" -> Some Applied
    | "Interview" -> Some Interview
    | "Rejected" -> Some Rejected
    | _ -> None

let validateApplication company position status =
    if String.IsNullOrWhiteSpace(company) then
        Some "Company is required."
    elif String.IsNullOrWhiteSpace(position) then
        Some "Position is required."
    elif Option.isNone (parseStatus status) then
        Some "Invalid status selected."
    else
        None