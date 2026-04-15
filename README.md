# CareerTrack

CareerTrack is a job and internship application tracking web application built with F#.

## Features

- Add new job applications
- View applications in a table
- Edit and delete applications
- Search applications by company, position, and notes
- Filter by status (Applied, Interview, Rejected)
- Sort applications by date or company
- View application statistics

## Technologies

- F#
- ASP.NET Core Minimal API
- HTML & CSS

## How to Run

1. Open the project in Visual Studio
2. Build and run the application
3. Open in browser:
   `https://localhost:5001/applications-page`

## Project Structure

- `Program.fs` - ASP.NET Core routes and page rendering
- `Domain.fs` - domain model, validation, filtering, and statistics logic
- In-memory storage (no database)

## Purpose

This project demonstrates CRUD operations, domain modeling, filtering, validation, and statistics calculation in an F# web application.

## Screenshots

### Applications Page
![Applications](screenshots/applications.png)

### Add Application
![Add](screenshots/add.png)

### Statistics
![Stats](screenshots/stats.png)

## Live Demo

Live demo link will be added after deployment.