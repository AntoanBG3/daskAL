# 🏫 daskAL

**daskAL** is a modern, web-based School Management System built with **.NET 9** and **Blazor**. It streamlines school administration by providing role-based access for Administrators, Teachers, and Students to manage grades, classes, subjects, and internal communication.

![.NET](https://img.shields.io/badge/.NET-9.0-512bd4?style=flat&logo=dotnet)
![Blazor](https://img.shields.io/badge/Blazor-Web-512bd4?style=flat&logo=blazor)
![SQLite](https://img.shields.io/badge/SQLite-Database-003B57?style=flat&logo=sqlite)
![License](https://img.shields.io/badge/License-MIT-green.svg)

---

## ✨ Features

### 🔐 Secure Role-Based Access
*   **Administrator:** Full control over the system. Manage users, classes, subjects, and view all data.
*   **Teacher:** Manage student grades, view assigned subjects, record absences, and communicate with students.
*   **Student:** View personal dashboard, check grades, see assigned teachers, and access school messages.

### 📚 Academic Management
*   **Class & Subject Management:** Organize school classes and curriculum subjects easily.
*   **Grade Recording:** Teachers can record and update grades (2-6 scale) for their specific subjects.
*   **Absence Tracking:** Monitor student attendance (Excused/Unexcused).

### 💬 Communication
*   **Internal Messaging:** Built-in secure messaging system between teachers, students, and admins.

### 🛠 Technical Highlights
*   **Clean Architecture:** Separation of concerns with dedicated Services and Models.
*   **EF Core & SQLite:** Robust data persistence with easy-to-setup database.
*   **Responsive UI:** Built with Bootstrap 5 for a consistent experience across devices.

---

## 🚀 Getting Started

### Prerequisites
*   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) installed on your machine.

### Installation

1.  **Clone the repository**
    ```bash
    git clone https://github.com/AntoanBG3/daskAL.git
    cd daskAL
    ```

2.  **Navigate to the Web Project**
    ```bash
    cd SchoolManagementSystem.Web/SchoolManagementSystem.Web
    ```

3.  **Run the Application**
    ```bash
    dotnet run
    ```
    *The database will be automatically created and seeded with default data upon first run.*

4.  **Access the App**
    Open your browser and navigate to `http://localhost:5000` (or the URL shown in your console).

---

## 🔑 Default Login Credentials

For testing purposes, the system is pre-seeded with the following accounts:

| Role | Email | Password |
| :--- | :--- | :--- |
| **Admin** | `admin@school.com` | `Admin123!` |

> **Note:** Please change these credentials or delete these accounts in a production environment.

---

## 🏗 Project Structure

```text
daskAL/
├── SchoolManagementSystem.ConsoleApp/    # Legacy Console Application
├── SchoolManagementSystem.Web/           # Main Web Solution
│   ├── SchoolManagementSystem.Web/       # Blazor Server Web App
│   │   ├── Components/                   # Razor Components (Pages, Layouts)
│   │   ├── Data/                         # EF Core Context & Migrations
│   │   ├── Models/                       # Domain Entities
│   │   ├── Services/                     # Business Logic & Data Access
│   │   └── wwwroot/                      # Static Assets (CSS, JS)
│   └── SchoolManagementSystem.Web.Client/# Blazor Client (WASM) Project
```

## 📜 License

This project is licensed under the [MIT License](LICENSE).