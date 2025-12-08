# Network Testing Instructions

This document explains how to host the School Management System on your local network so you can access it from other devices (e.g., your phone, another laptop).

## Prerequisites

*   You must be connected to the same network (Wi-Fi/LAN) as the device you want to test with.
*   You need to know your computer's local IP address.
    *   **MacOS**: Go to **System Settings > Network > Wi-Fi** (or Ethernet), click **Details**, and look for **IP Address**.

---

## Option 1: Using Docker (Recommended)

This method runs the application in an isolated container.

1.  **Build the Docker image**:
    Open your terminal in the **root** of the repository (where `SchoolManagementSystem.sln` and `Dockerfile` are) and run:
    ```bash
    docker build -t school-system .
    ```

2.  **Run the container**:
    ```bash
    docker run -p 5093:8080 -e ASPNETCORE_ENVIRONMENT=Development school-system
    ```
    *Note: We map port 5093 on your machine to port 8080 in the container.*

3.  **Access the site**:
    Open a browser on another device and go to: `http://<YOUR_IP>:5093`

### Debugging Docker
If the container exits immediately or you cannot connect:
1.  Check the status: `docker ps -a`
2.  View the logs to see why it crashed:
    ```bash
    docker logs <CONTAINER_ID>
    ```
    *(Replace `<CONTAINER_ID>` with the ID from the previous command).*

---

## Option 2: Using `dotnet run` script

This method runs the application directly on your machine. This is often easier if Docker gives you trouble.

1.  **Run the script**:
    In the root of the repository, run:
    ```bash
    ./run-network.sh
    ```

2.  **Access the site**:
    Open a browser on another device and go to: `http://<YOUR_IP>:5093`

---

## Troubleshooting

*   **Firewall**: If you cannot connect, check if your MacOS Firewall is blocking incoming connections. You might need to allow the application or turn off the firewall temporarily for testing.
*   **Database**: The application uses a local SQLite database (`school.db`).
    *   In **Option 2**, it will use the file in `SchoolManagementSystem.Web/school.db`.
    *   In **Option 1 (Docker)**, the database is inside the container. We have configured the permissions so it can be created automatically. Note that data is lost when you remove the container unless you mount a volume.
