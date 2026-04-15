# CareerTrack 🚀

A web application for tracking job and internship applications, built with F# and ASP.NET Core.

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

## Tech Stack

* F#
* ASP.NET Core
* .NET 9
* HTML & CSS
* Docker
* Render (deployment)

---

## Screenshots

![Home](screenshots/home.png)
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
