# CareerTrack

A web application for tracking job and internship applications, built with F# and ASP.NET Core.

---

## About the Project

This application helps track job applications in a structured way, providing useful insights such as application statistics, rejection rates, and recent activity.

The goal was to build a practical tool with a clean UI and meaningful features beyond basic CRUD operations.

---

## Live Demo

https://careertrack-e5rg.onrender.com

---

## Features

* Add new job applications
* View applications in a structured table
* Edit and delete entries
* Filter by status (Applied, Interview, Rejected)
* Search by company or position
* Statistics dashboard

---

## Extra Features

- Favorites system ⭐
- Dashboard cards for quick overview
- Recent applications panel
- Days since applied tracking
- Highlighting old applications
- Rejection rate calculation
- Most common status detection

---

## UX Highlights

- Clean and minimal interface
- Color-coded status indicators for better readability
- Quick filtering and searching for faster navigation
- Visual statistics with progress bars and charts
- Responsive layout for different screen sizes

---

## Tech Stack

* F#
* ASP.NET Core
* .NET 9
* HTML & CSS
* Docker
* Render (deployment)

---

## Screenshots

![Applications](screenshots/applications.png)
![Add](screenshots/add.png)
![Stats](screenshots/stats.png)

---

## Getting Started (Local)

```bash
git clone https://github.com/elekvk/careertrack.git
cd careertrack
dotnet run
```

---

## Run with Docker

```bash
docker build -t careertrack .
docker run -p 10000:10000 careertrack
```

---

## Deployment

This project is deployed using Render with Docker.

---

## Author

* GitHub: https://github.com/elekvk
