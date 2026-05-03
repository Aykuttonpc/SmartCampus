<div align="center">

# SmartCampus

**Real-time university campus space management & reservation system**

![.NET](https://img.shields.io/badge/.NET_8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)

[Features](#-features) · [Architecture](#-architecture) · [Database Design](#-database-design) · [Setup](#-setup) · [Security](#-security)

</div>

---

## Overview

SmartCampus is a full-stack ASP.NET Core MVC application for managing university campus spaces — libraries, labs, sports halls, and meeting rooms. It provides **live occupancy monitoring** via sensor data, a **role-based reservation system**, and an **admin analytics panel**.

> Built with a T-SQL-heavy approach: complex Views, Stored Procedures, and server-side pagination handle the data layer while EF Core manages ORM concerns.

---

## Features

| Feature | Description |
|---------|-------------|
| **Live Dashboard** | Real-time occupancy % for all spaces with crowd-level indicators (Empty → Over Capacity) |
| **Reservation System** | Book 1-hour slots (08:00–22:00) with automatic conflict detection |
| **Role-based Access** | Student / Staff / Admin with fine-grained per-action permissions |
| **Booking Calendar** | Today's reservations with status-color coding (Pending / Approved / Cancelled / Completed) |
| **Alternative Finder** | Suggests least-busy spaces of the same type when a space is full |
| **Admin Analytics** | No-show report with risk classification (Good Standing / Moderate Risk / High Risk) |
| **Penalty System** | Tracks no-show penalties per user, visible on profile |
| **Server-side Pagination** | Handles large occupancy log datasets efficiently |

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Web Framework | ASP.NET Core MVC (.NET 8) |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Real-time | SignalR |
| Authentication | Session-based (`AddSession` middleware) |
| UI | Razor Views + Vanilla CSS |
| Container | Docker |

---

## Architecture

```
SmartCampus/
├── Controllers/     # MVC controllers (Account, Home, Reservations, Admin)
├── Data/            # DbContext and database configuration
├── Hubs/            # SignalR hubs for real-time occupancy updates
├── Migrations/      # EF Core database migrations
├── Models/          # Domain entities
├── Services/        # Business logic layer
├── Views/           # Razor templates
└── wwwroot/         # Static assets (CSS, JS)
```

**Request flow:**
```
Browser → Controller → Service → DbContext / Stored Procedure → SQL Server
                                                  ↑
                                        SignalR Hub (real-time push)
```

---

## Database Design

```
Roles ──────────────── UserRoles ─────────────── Users
                                                    │
FacilityTypes ──── Facilities ──── Sensors ──── OccupancyLogs
                        │
                   Reservations ──── Statuses
                        │
           ReservationAttendees · UserPenalties · ReservationAuditLogs
```

### Key SQL Objects

| Object | Type | Purpose |
|--------|------|---------|
| `vw_LiveOccupancy` | View | Latest sensor reading + occupancy % per facility |
| `vw_TodayReservations` | View | Today's reservations with organizer and status data |
| `sp_FindFreeSlots` | Stored Procedure | Available 1-hour slots for a facility on a given date |
| `sp_GetUserDashboard` | Stored Procedure | Full user profile stats in a single round-trip |
| `sp_SuggestAlternatives` | Stored Procedure | Least-busy facilities of the same type |
| `sp_GetNoShowReport` | Stored Procedure | No-show analytics with risk classification |

---

## User Roles

| Action | Student | Staff | Admin |
|--------|:-------:|:-----:|:-----:|
| View live dashboard | ✅ | ✅ | ✅ |
| Make reservation | ✅ | ✅ | ✅ |
| Cancel **own** reservation | ✅ | ✅ | ✅ |
| Cancel **any** reservation | ❌ | ✅ | ✅ |
| Approve reservations | ❌ | ✅ | ✅ |
| No-show analytics panel | ❌ | ❌ | ✅ |
| Facility management | ❌ | ❌ | ✅ |

---

## Setup

**Prerequisites:** .NET 8 SDK · SQL Server · SSMS (optional)

```bash
# 1. Clone
git clone https://github.com/Aykuttonpc/SmartCampus.git
cd SmartCampus

# 2. Create database — run SmartCampusDB.sql in SSMS
#    (creates schema + seed data with test users)

# 3. Update connection string in appsettings.json
#    "DefaultConnection": "Server=YOUR_SERVER;Database=SmartCampusDB;Trusted_Connection=True;"

# 4. Run
dotnet run --project SmartCampus/SmartCampus.csproj

# 5. Open http://localhost:5000
```

**Test credentials (seed data):**

| Role | Email | Password |
|------|-------|----------|
| Student | `kaan1@uni.edu` | `hash` |
| Staff | *(check seed data)* | `hash` |
| Admin | *(check seed data)* | `hash` |

**Docker:**
```bash
docker build -t smartcampus .
docker run -p 5000:5000 smartcampus
```

---

## Security

| Mechanism | Implementation |
|-----------|---------------|
| **IDOR Protection** | Reservation cancellation validates `OrganizerUserID == currentUser` server-side |
| **Auth Guard** | Protected routes check session; redirect to `/Account/Login` if absent |
| **Role Enforcement** | Admin/Staff actions validated at controller level, not just UI |
| **SQL Injection** | All queries parameterized through EF Core; raw T-SQL via stored procedures with parameters |
| **Input Validation** | HTML5 + server-side length constraints matching DB column sizes |

---

## URL Reference

| Route | Method | Access |
|-------|--------|--------|
| `/` | GET | Public |
| `/Account/Login` | GET · POST | Public |
| `/Account/Profile` | GET | Auth required |
| `/Reservations` | GET | Auth required |
| `/Reservations/Book` | GET | Auth required |
| `/Reservations/Confirm` | POST | Auth required |
| `/Reservations/Cancel/{id}` | POST | Owner or Staff/Admin |
| `/Reservations/Approve/{id}` | POST | Staff/Admin |
| `/Reservations/Alternatives` | GET | Auth required |
| `/Admin` | GET | Admin only |
| `/Admin/Facilities` | GET · POST | Admin only |
