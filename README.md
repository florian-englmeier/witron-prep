# WITRON Sensor Monitor

> Full-stack web application for monitoring and alerting of intralogistics sensors  
> C# | ASP.NET Core | JavaScript | HTML/CSS

---

## Overview

This project simulates an industrial monitoring system for logistics facilities — inspired by real-world requirements in intralogistics and warehouse automation. It consists of two parts:

**1. Console App (HelloWITRON):** C# fundamentals including OOP, inheritance, interfaces, LINQ, and exception handling — applied to sensor technology and measurement.

**2. Web API + Dashboard (SensorAPI):** ASP.NET Core REST API with a live dashboard in the browser. Sensor data is displayed in real time, alarms are color-coded, and technicians can acknowledge alerts directly.

---

## Features

**Backend (C# / ASP.NET Core):**
- REST API with GET and POST endpoints
- Intralogistics sensors: RPM, vibration, torque, voltage, load movement
- Sensor grouping (Drives, Electrical, Conveyor, Maintenance)
- Alarm detection on threshold violation
- Alarm acknowledgment with technician name and timestamp
- CSV file logging with exception handling
- Async/await for HTTP communication

**Frontend (JavaScript / HTML / CSS):**
- Live dashboard with auto-refresh (2 seconds)
- Color-coded measurement bars (green = OK, red = alarm)
- Status overview: sensors active / OK / alarms
- Acknowledge button on alarm with input dialog
- Industrial dark theme

**Console App (C#):**
- Object-oriented programming with inheritance
- Base class Sensor with child classes (TemperaturSensor, DruckSensor)
- Interface ISensor
- LINQ queries (Average, Min, Max, Where)
- Color-coded console output for alarm checks

---

## Tech Stack

| Technology | Usage |
|-----------|-------|
| C# / .NET 10 | Backend logic, API controllers |
| ASP.NET Core | REST API, middleware pipeline |
| JavaScript | Dashboard frontend, fetch() API |
| HTML / CSS | UI layout, dark theme |
| Git | Version control with semantic versioning |

---

## API Endpoints

| Method | URL | Description |
|--------|-----|------------|
| GET | `/api/sensor` | All sensors with current readings |
| GET | `/api/sensor/alarm` | Sensors in alarm state only |
| POST | `/api/sensor/quittieren` | Acknowledge alarm (Body: SensorName, Techniker) |

---

## Sensor Types (Intralogistics)

| Sensor | Unit | Range | Group |
|--------|------|-------|-------|
| Drive-1 Speed | RPM | 0 – 1500 | Drives |
| Drive-1 Vibration | mm/s | 0 – 4.5 (ISO 10816) | Drives |
| Drive-2 Torque | Nm | 0 – 120 | Drives |
| Voltage Zone-A | V | 22 – 26 (24V DC ±10%) | Electrical |
| Load Movement Zone-A | kg | 0 – 500 | Conveyor |
| Load Movement Zone-B | kg | 0 – 500 | Conveyor |
| Operating Hours | h | 0 – 8760 | Maintenance |

---

## Project Structure

```
witron-prep/
├── HelloWITRON/              # Console App (C# fundamentals)
│   └── Program.cs            # OOP, inheritance, LINQ, alarms
├── SensorAPI/                # ASP.NET Core Web API
│   ├── wwwroot/
│   │   └── index.html        # Dashboard frontend
│   ├── SensorController.cs   # API controller (GET, POST)
│   └── Program.cs            # Middleware pipeline
├── TUTORIAL.md               # Personal learning journal
└── README.md
```

---

## Version History

| Version | Description |
|---------|------------|
| v0.1.0 | Hello WITRON — first sensor simulator |
| v0.2.0 | List and foreach |
| v0.3.0 | Inheritance, override, alarm system with color output |
| v0.4.0 | Interface ISensor + LINQ queries |
| v0.5.0 | CSV file logging with exception handling |
| v0.6.0 | Async/await + HTTP weather data |
| v0.7.0 | ASP.NET Core Web API |
| v0.8.0 | HTML/JS dashboard + alarm endpoint |
| v0.9.0 | Alarm acknowledgment + auto-refresh |
| v0.10.0 | Intralogistics sensors with grouping |

---

## Getting Started

```bash
# Clone the repository
git clone https://github.com/florian-englmeier/witron-prep.git

# Run the console app
cd HelloWITRON
dotnet run

# Run the Web API + Dashboard
cd SensorAPI
dotnet run
# Open browser: https://localhost:7111/index.html
```

**Requirements:** .NET 10 SDK, Visual Studio 2022 (optional)

---

## Background

This project was developed as hands-on preparation for a web application development role in industrial logistics automation. All code was written independently — step by step, documented through the commit history.

**Author:** Florian Englmeier  
**Education:** M.Eng. Electronic & Mechatronic Systems, Dipl.-Ing. Physical Engineering  
**Experience:** 10+ years in industry (robotics, measurement technology, process optimization)
