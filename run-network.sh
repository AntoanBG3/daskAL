#!/bin/bash
# Script to run the application on the local network using dotnet run

echo "Starting SchoolManagementSystem on 0.0.0.0:5093..."
echo "You can access it from other devices at http://<YOUR_IP>:5093"

# Navigate to the web project directory
# Root -> SchoolManagementSystem.Web (Solution Dir) -> SchoolManagementSystem.Web (Project Dir)
cd SchoolManagementSystem.Web/SchoolManagementSystem.Web || { echo "Directory not found!"; exit 1; }

# Run the application binding to all interfaces
dotnet run --urls "http://0.0.0.0:5093"
