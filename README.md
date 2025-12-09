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
*   **Email Verification:** New users must confirm their email address before accessing the system.
*   **Account Approval:** New student accounts require administrator approval after email verification.

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

3.  **Database Setup (Admin Seeding)**

    The application does not automatically create an admin account by default. You must run the application with the `--seed-admin` flag once to create the initial administrator account.

    ```bash
    dotnet run --seed-admin
    ```
    *This will create the database (if missing), apply migrations, and seed the default admin account. It will then start the application.*

    If you want to only seed and exit:
    ```bash
    dotnet run --seed-admin --only-seed
    ```

4.  **Run the Application**
    After the initial setup, you can run the application normally:
    ```bash
    dotnet run
    ```

5.  **Access the App**
    Open your browser and navigate to `http://localhost:5000` (or the URL shown in your console).

### 📧 Email Verification (Development)

In the development environment, no actual emails are sent. Instead, the email confirmation link is logged to the **console output**.

1.  Register a new user.
2.  Check the console where `dotnet run` is executing.
3.  Look for a log entry starting with `Sending email to...`.
4.  Copy the confirmation link (e.g., `http://localhost:5000/confirm-email?userId=...`) and paste it into your browser to verify the account.

---

## 🔑 Default Login Credentials

If you ran the seeding command, the following admin account is created:

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
