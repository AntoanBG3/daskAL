# 🏫 daskAL

**daskAL** is a modern, web-based School Management System built with **.NET 9** and **Blazor**. It provides role-based access for Administrators, Teachers, and Students to manage grades, classes, subjects, schedules, and internal communication.

![.NET](https://img.shields.io/badge/.NET-9.0-512bd4?style=flat&logo=dotnet)
![Blazor](https://img.shields.io/badge/Blazor-Web-512bd4?style=flat&logo=blazor)
![SQLite](https://img.shields.io/badge/SQLite-Database-003B57?style=flat&logo=sqlite)
![License](https://img.shields.io/badge/License-MIT-green.svg)

---

## ✨ Features

### 🔐 Role-Based Access
- **Administrator** — Full system control: manage users, approve registrations, oversee all data.
- **Teacher** — Record grades & absences, view assigned subjects, communicate with students.
- **Student** — Personal dashboard with grades, schedule, assigned teachers, and messaging.

### 📚 Academic Management
- **Classes & Subjects** — Organize curriculum with many-to-many class–subject assignments.
- **Grade Recording** — Teachers record and update grades (2–6 scale) per subject.
- **Absence Tracking** — Monitor attendance with excused/unexcused status.
- **Schedule Management** — Weekly timetable with conflict detection (teacher, room, class).

### 💬 Communication
- **Internal Messaging** — Secure messaging between all user roles with read/unread tracking.

### � Technical Highlights
- **Interface-Driven Services** — All business logic behind interfaces (`IClassService`, `ITeacherService`, etc.) for testability and clean DI.
- **EF Core & SQLite** — Code-first migrations with automatic database creation on first run.
- **ASP.NET Core Identity** — Authentication with login attempt logging and admin-approved registration.
- **Blazor Server + WebAssembly** — Interactive UI with server-side rendering and client-side components.
- **Unit Tested** — xUnit + Moq test suite covering service-layer operations.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/AntoanBG3/daskAL.git
   cd daskAL
   ```

2. **Run the application**
   ```bash
   cd SchoolManagementSystem.Web/SchoolManagementSystem.Web
   dotnet run
   ```
   The database is automatically created and seeded on first run.

3. **Open in browser**
   Navigate to the URL shown in your console (typically `http://localhost:5000`).

---

## 🔑 Default Credentials

| Role | Email | Password |
| :--- | :--- | :--- |
| **Admin** | `admin@school.com` | `Admin123!` |

> **Note:** Change these credentials before deploying to production. New teacher and student accounts require admin approval after registration.

---

## 🏗 Project Structure

```text
daskAL/
├── SchoolManagementSystem.Web/
│   ├── SchoolManagementSystem.Web/           # Blazor Server App
│   │   ├── Components/                       # Razor Pages & Layouts
│   │   ├── Controllers/                      # API Controllers (Schedule)
│   │   ├── Data/                             # EF Core Context, Migrations & Seeder
│   │   ├── DTOs/                             # Data Transfer Objects
│   │   ├── Models/                           # Domain Entities & ViewModels
│   │   ├── Services/                         # Interfaces & Implementations
│   │   └── wwwroot/                          # Static Assets (CSS, JS)
│   ├── SchoolManagementSystem.Web.Client/    # Blazor WASM Client
│   └── SchoolManagementSystem.Tests/         # xUnit Test Suite
└── SchoolManagementSystem.ConsoleApp/        # Legacy Console App
```

---

## 🧪 Running Tests

```bash
dotnet test SchoolManagementSystem.Web/SchoolManagementSystem.Web.sln
```

---

## 📜 License

This project is licensed under the [MIT License](LICENSE).