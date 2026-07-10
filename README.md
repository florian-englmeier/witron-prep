# WITRON Sensor Monitor

> **Full-stack industrial automation project — from TwinCAT PLC to Web Dashboard**
> C# | ASP.NET Core | TwinCAT 3 | Beckhoff ADS | JavaScript | HTML/CSS

---

## Overview

This project bridges **industrial PLC control** and a **modern web application** — inspired by real-world requirements in intralogistics and warehouse automation.

A Beckhoff **CP6606 PLC** runs a step-chain process (pallet handling with lifting, temperature check, and alarm handling). A .NET Web API connects to the PLC via **Beckhoff ADS** and exposes the live data as REST endpoints. A browser dashboard consumes the API and visualizes the process in real time.

The project consists of three parts:

1. **PalettenStation (TwinCAT 3)** — Structured Text PLC program with a 10-state process chain (S0–S9), simulation mode, and alarm logic. Runs on real Beckhoff hardware.
2. **SensorAPI (.NET / ASP.NET Core)** — REST API acting as the bridge. Uses the Beckhoff.TwinCAT.Ads NuGet package to read live values from the PLC.
3. **HelloWITRON (C# Console)** — teaching-oriented sandbox for OOP fundamentals, LINQ, async/await, and exception handling.

---

## Key Features

### PLC layer (TwinCAT 3, Structured Text)
- Full step chain **S0–S9** including error branch and operator acknowledgment
- Clean separation between **hardware inputs** and **simulation values** via `GVL_IO.SIM_Aktiv`
- Analog process values (foil temperature, motor temperature)
- Runs on physical Beckhoff hardware (**CP6606** with **EK1100** EtherCAT coupler)

### Communication layer (Beckhoff ADS)
- Symbolic addressing — no hard-coded memory addresses
- `AdsService` in ASP.NET Core with typed read methods (`ReadBool`, `ReadReal`, `ReadInt`)
- Guard clauses and exception handling for production-grade robustness
- Registered as **Singleton** in ASP.NET dependency injection

### Web API (C# / ASP.NET Core)
- REST endpoints returning live PLC data as JSON
- **Live endpoint** with 4 PLC variables including step-chain state translation
- **Health endpoint** for ADS connection monitoring
- Legacy endpoints for simulated intralogistics sensors (RPM, vibration, torque)
- CSV file logging with exception handling
- Alarm acknowledgment with technician name and timestamp

### Frontend (JavaScript / HTML / CSS)
- Live dashboard with auto-refresh
- Color-coded measurement bars (green = OK, red = alarm)
- Industrial dark theme
- Alarm acknowledgment button with input dialog

---

## Tech Stack

| Technology | Usage |
|------------|-------|
| **TwinCAT 3** | PLC runtime on Beckhoff CP6606, Structured Text |
| **Beckhoff.TwinCAT.Ads 7.0.292** | ADS client for .NET |
| **C# / .NET 10** | Backend logic, API controllers |
| **ASP.NET Core** | REST API, middleware pipeline, dependency injection |
| **JavaScript** | Dashboard frontend, `fetch()` API |
| **HTML / CSS** | UI layout, dark theme |
| **Git** | Version control with semantic versioning |

---

## Hardware

| Component | Role |
|-----------|------|
| Beckhoff CP6606 Panel PC | TwinCAT 3 Runtime (ARM, Windows CE) |
| Beckhoff EK1100 | EtherCAT bus coupler |
| Digital I/O terminals (DI/DO) | Palette sensors, drives, indicators |
| Analog I/O terminals (AI/AO) | Temperature, level, setpoints |
| Beckhoff EL3681 | Digital multimeter terminal (voltage/current) — planned for v1.3.0 |

---

## API Endpoints

### Live PLC data (via ADS)
| Method | URL | Description |
|--------|-----|-------------|
| GET | `/api/sensor/ads-status` | ADS connection health and PLC state |
| GET | `/api/sensor/live` | Live values from PalettenStation PLC (step, temperatures, alarm) |

### Simulated intralogistics sensors
| Method | URL | Description |
|--------|-----|-------------|
| GET | `/api/sensor` | All simulated sensors with current readings |
| GET | `/api/sensor/alarm` | Sensors in alarm state only |
| POST | `/api/sensor/quittieren` | Acknowledge alarm (Body: `SensorName`, `Techniker`) |

### Example — `/api/sensor/live` response

```json
{
  "connected": true,
  "schritt": 5,
  "schrittText": "S5 - Bearbeitung laeuft",
  "folienTemperatur": 175.2,
  "motorTemperatur": 48.7,
  "alarmAktiv": false,
  "zeitstempel": "22:47:13"
}
```

---

## PLC Step Chain (PalettenStation)

```text
S0 Ready  →  S1 Transport  →  S2 Stop  →  S3 Lift up  →  S4 Temp check
                                                              ↓
              S7 Discharge  ←  S6 Lift down  ←  S5 Process (5s)
                     ↓
                (back to S0)

Error branch:  S8 Alarm  →  S9 Wait for acknowledgment  →  S0
```

---

## Project Structure

```text
witron-prep/
├── HelloWITRON/                          # C# fundamentals sandbox
│   └── Program.cs                        # OOP, inheritance, LINQ
├── SensorAPI/                            # ASP.NET Core Web API
│   ├── wwwroot/
│   │   └── index.html                    # Dashboard frontend
│   ├── AdsService.cs                     # ADS client (Singleton)
│   ├── SensorController.cs               # API endpoints
│   ├── Program.cs                        # Middleware pipeline
│   └── SensorAPI.csproj                  # NuGet: Beckhoff.TwinCAT.Ads
├── TwinCAT/                              # TwinCAT XAE project
│   └── PalettenStation/                  # PLC program (GVL_IO, MAIN, ...)
├── TUTORIAL.md                           # C# learning journal (v0.1 – v0.9)
├── TUTORIAL_v1.0_TwinCAT_Roadmap.md      # Strategy, hardware, phases
├── TUTORIAL_v1.2_ADS_Integration.md      # Deep-dive: PLC ↔ .NET bridge
└── README.md
```

---

## Documentation Guide

The project has **three tutorials**, each with a distinct purpose:

| Document | For whom | Content |
|----------|----------|---------|
| `TUTORIAL.md` | C# learners | 15 chapters from Hello World through OOP, LINQ, async, ASP.NET, and JS dashboard |
| `TUTORIAL_v1.0_TwinCAT_Roadmap.md` | PLC-side / project managers | Anlagenlogik, hardware, EL3681 planning, phase roadmap |
| `TUTORIAL_v1.2_ADS_Integration.md` | Full-stack integrators | The bridge: symbolic addressing, PLC↔.NET type mapping, silent-bug traps |

---

## Version History

| Version | Milestone |
|---------|-----------|
| v0.1 – v0.9 | C# fundamentals, ASP.NET, HTML/JS dashboard (Hello WITRON → simulated sensors) |
| v0.10 | Intralogistics sensors with grouping (Drives, Electrical, Conveyor) |
| v1.0 | TwinCAT PalettenStation project — GVL_IO, GVL_Simulation, step chain S0-S9 |
| v1.1 | AdsService scaffolding, ADS router troubleshooting |
| **v1.2** | **ADS integration with live PLC data (this release)** |
| v1.3 *(planned)* | EL3681 integration — real voltage/current measurement |
| v1.4 *(planned)* | Predictive maintenance feature — trend logging and diagnostic rules |

---

## Getting Started

### Requirements
- .NET 10 SDK
- Visual Studio 2022 (latest) — or Rider
- TwinCAT 3 XAE (optional, for PLC development)
- A Beckhoff PLC or valid ADS route (optional, for live PLC data)

### Clone and run
```bash
# Clone the repository
git clone https://github.com/florian-englmeier/witron-prep.git

# Run the Web API + Dashboard
cd witron-prep/SensorAPI
dotnet run
# Open browser: https://localhost:7111/index.html

# For live PLC endpoints (/api/sensor/live), an ADS route to a running
# TwinCAT PLC with the PalettenStation project is required.
# Without one, the endpoint reports connected=true, adsState=Invalid.

# Run the C# sandbox
cd ../HelloWITRON
dotnet run
```

---

## Learning Highlights

Skills exercised across this project:

- **Industrial automation:** IEC 61131-3 Structured Text, grafcet-style step chains, alarm handling
- **Hardware integration:** Beckhoff CP6606, EtherCAT (EK1100 + I/O terminals), ADS route configuration
- **Systems programming:** Symbolic PLC addressing, type mapping between IEC 61131 and .NET (with awareness of the `INT` vs `int` trap)
- **Backend:** ASP.NET Core middleware, dependency injection, REST design, exception handling patterns
- **Frontend:** `fetch()` API, JSON parsing, auto-refresh with `setInterval`, DOM manipulation
- **DevOps:** Git with semantic versioning, meaningful commit messages, annotated tags

---

## Background

This project was developed as hands-on preparation for a web application development role in industrial logistics automation. All code was written independently, step by step, documented through the commit history and three companion tutorials.

The scope covers the **full vertical stack** of an industrial monitoring system: from a real PLC on a real EtherCAT bus, through a .NET service using an industrial fieldbus protocol, up to a browser dashboard — all built and documented in the open.

**Author:** Florian Englmeier
**Education:** M.Eng. Electronic & Mechatronic Systems, Dipl.-Ing. Physical Engineering
**Experience:** 10+ years in industry (robotics, measurement technology, process optimization)
