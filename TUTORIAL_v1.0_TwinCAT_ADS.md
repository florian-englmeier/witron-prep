# TUTORIAL Kapitel v1.0 — TwinCAT \+ C\# ADS Integration

**Anschluss an:** TUTORIAL.md v0.9.0  
**Stand:** Juli 2026  
**Umgebung:** TwinCAT 3 XAE auf nativem Windows-PC | C\# ASP.NET Core auf separatem Rechner  
**Ziel:** Palettenstation-Simulation — C\# berechnet Werte, TwinCAT führt Logik aus, Dashboard zeigt alles

---

## Inhaltsverzeichnis

1. [Architektur-Überblick](#1-architektur-überblick)  
2. [Hardware — Klemmenübersicht](#2-hardware--klemmenübersicht)  
3. [GVL\_IO — Globale Ein-/Ausgänge](#3-gvl_io--globale-ein-ausgänge)  
4. [GVL\_Simulation — Simulationswerte](#4-gvl_simulation--simulationswerte)  
5. [PRG\_Palette — Schrittfolge S0–S9](#5-prg_palette--schrittfolge-s0s9)  
6. [Programm als Task registrieren und aktivieren](#6-programm-als-task-registrieren-und-aktivieren)  
7. [C\# ADS-Verbindung aufbauen](#7-c-ads-verbindung-aufbauen)  
8. [C\# Simulationswerte berechnen und schreiben](#8-c-simulationswerte-berechnen-und-schreiben)  
9. [Dashboard erweitern](#9-dashboard-erweitern)  
10. [Grenzwerte Referenz](#10-grenzwerte-referenz)  
11. [Typische Fehler TwinCAT & ADS](#11-typische-fehler-twincat--ads)

---

## 1\. Architektur-Überblick

┌─────────────────────────────────────────────────────┐

│          witron-prep Dashboard (Browser)             │

│   Temperatur | Schritte S0-S9 | Alarme | DI/DO      │

└──────────────────────┬──────────────────────────────┘

                       │ HTTP / JSON (fetch alle 2s)

┌──────────────────────▼──────────────────────────────┐

│          ASP.NET Core Web-API (C\#)                   │

│  \- Simulationswerte berechnen (Sinus, Rampen)        │

│  \- Werte per ADS → TwinCAT schreiben                 │

│  \- SPS-Zustand per ADS ← TwinCAT lesen              │

└──────────────────────┬──────────────────────────────┘

                       │ ADS (Beckhoff.TwinCAT.Ads NuGet)

┌──────────────────────▼──────────────────────────────┐

│          TwinCAT 3 auf CP6606                        │

│  IP: 192.168.1.10 | AMS Net ID: 5.35.203.54.1.1    │

│  \- Schrittfolge S0-S9 in Structured Text            │

│  \- Grenzwert-Logik (Warn / Alarm)                   │

│  \- GVL\_IO mit AT %I\* / %Q\* Hardware-Mapping         │

│  \- GVL\_Simulation für Testbetrieb ohne Hardware     │

└─────────────────────────────────────────────────────┘

**Warum diese Aufteilung?**

In echten WITRON-Anlagen schreibt der Leitrechner Sollwerte in die SPS — die SPS macht die Sicherheits- und Schrittlogik. C\# übernimmt hier die Leitrechner-Rolle. Das ist kein Trick für das Portfolio, das ist die echte Architektur im Kleinen.

---

## 2\. Hardware — Klemmenübersicht

| Klemme | Typ | Kanäle | Verwendung |
| :---- | :---- | :---- | :---- |
| EK1100 | EtherCAT Coupler | — | Bus-Koppler |
| EL1002 | DI 24V | 2 ch | Digitale Eingänge |
| EL1004 | DI 24V | 4 ch | Digitale Eingänge |
| EL1004 | DI 24V | 4 ch | Digitale Eingänge |
| EL1008 | DI 24V | 8 ch | Digitale Eingänge |
| EL2004 | DO 24V | 4 ch | Digitale Ausgänge |
| EL2004 | DO 24V | 4 ch | Digitale Ausgänge |
| EL2008 | DO 24V | 8 ch | Digitale Ausgänge |
| EL3064 | AI 0–10V | 4 ch | Analoge Eingänge Spannung |
| EL3144 | AI 4–20mA | 4 ch | Analoge Eingänge Strom |
| EL3204 | AI PT100/PT1000 | 4 ch | Motortemperatur (AI\_MotorTemperatur) |
| EL3681 | AI Multimeter | 1 ch | Stromaufnahme Förderband (AI\_AntriebStrom) |
| EL4014 | AO 4–20mA | 4 ch | Analoge Ausgänge |

**Gesamt:** 18 DI | 16 DO | 12 AI \+ 1 Multimeter | 4 AO

**EL3681 — Besonderheit:** Digitales Multimeter als EtherCAT-Klemme. Misst Strom (bis 10A) und Spannung (bis 300V AC/DC) mit automatischer Bereichsumschaltung. Wird als Stromüberwachung des Förderbandantriebs verwendet — Überstrom \= Palette klemmt, Unterstrom \= Förderband läuft leer.

---

## 3\. GVL\_IO — Globale Ein-/Ausgänge

**Aktueller Stand (produktiv):**

VAR\_GLOBAL

    // \==================================================

    // DIGITALE EINGÄNGE — AT %I\* \= Hardware-Mapping

    // \==================================================

    DI\_NotHaltFrei         AT %I\* : BOOL;

    DI\_SchutzhaubeZu       AT %I\* : BOOL;

    DI\_PaletteEingang      AT %I\* : BOOL;

    DI\_PalettePositioniert AT %I\* : BOOL;

    DI\_HubUnten            AT %I\* : BOOL;

    DI\_HubOben             AT %I\* : BOOL;

    DI\_KollisionFrei       AT %I\* : BOOL;

    DI\_WerkerQuittierung   AT %I\* : BOOL;

    // \==================================================

    // DIGITALE AUSGÄNGE — AT %Q\* \= Hardware-Mapping

    // \==================================================

    DO\_SignalGruen         AT %Q\* : BOOL;

    DO\_AlarmRot            AT %Q\* : BOOL;

    DO\_Foerderband         AT %Q\* : BOOL;

    DO\_FoerderbandStopp    AT %Q\* : BOOL;

    DO\_HubwerkHoch         AT %Q\* : BOOL;

    DO\_HubwerkRunter       AT %Q\* : BOOL;

    DO\_ProzessFreigabe     AT %Q\* : BOOL;

    DO\_Heizfreigabe        AT %Q\* : BOOL;

    // \==================================================

    // ANALOGE EINGÄNGE

    // KEIN AT %I\* — Werte kommen entweder von C\# per ADS

    // oder aus GVL\_Simulation (wenn SIM\_Aktiv \= TRUE)

    // AT %I\* würde Hardware den Wert überschreiben\!

    // \==================================================

    AI\_FolienTemperatur    : REAL;   // Soll 175°C, Bereich 165-185°C

    AI\_MotorTemperatur     : REAL;   // PT100 via EL3204, Warn 80°C, Stop 100°C

    AI\_AntriebStrom        : REAL;   // EL3681, Stromaufnahme Förderband in A

    // \==================================================

    // INTERNE PROGRAMMVARIABLEN

    // \==================================================

    Schritt    : INT  := 0;

    AlarmAktiv : BOOL := FALSE;

    SIM\_Aktiv  : BOOL := FALSE;   // TRUE \= Simulation, FALSE \= Hardware

END\_VAR

**Wichtig:** `AT %I*` und `AT %Q*` bedeuten dass TwinCAT die Variable automatisch auf die nächste freie Hardware-Adresse der passenden Klemme mappt. Die genaue Adresse ist im I/O-Baum unter dem jeweiligen Klemmen-Kanal sichtbar.

---

## 4\. GVL\_Simulation — Simulationswerte

Separate GVL für Testbetrieb — wird von C\# per ADS beschrieben. `{attribute 'qualified_only'}` erzwingt den Zugriff mit Präfix `GVL_Simulation.` — verhindert Namenskollisionen mit GVL\_IO.

{attribute 'qualified\_only'}

VAR\_GLOBAL

    // Digitale Eingänge — Startwerte \= sichere Grundstellung

    SIM\_NotHaltFrei         : BOOL := TRUE;

    SIM\_SchutzhaubeZu       : BOOL := TRUE;

    SIM\_PaletteEingang      : BOOL := FALSE;

    SIM\_PalettePositioniert : BOOL := FALSE;

    SIM\_HubUnten            : BOOL := TRUE;

    SIM\_HubOben             : BOOL := FALSE;

    SIM\_KollisionFrei       : BOOL := TRUE;

    SIM\_WerkerQuittierung   : BOOL := FALSE;

    // Analoge Werte — Startwerte \= Normalbetrieb

    SIM\_FolienTemperatur    : REAL := 175.0;   // Startwert \= Sollwert

    SIM\_MotorTemperatur     : REAL := 25.0;    // Startwert \= Raumtemperatur

    SIM\_AntriebStrom        : REAL := 0.0;     // Startwert \= kein Strom

END\_VAR

**Umschaltlogik Simulation/Hardware** — wird in PRG\_Palette am Anfang eingebaut:

// Am Anfang von PRG\_Palette, VOR der CASE-Struktur:

IF GVL\_IO.SIM\_Aktiv THEN

    // Simulationswerte in GVL\_IO kopieren

    GVL\_IO.DI\_NotHaltFrei         := GVL\_Simulation.SIM\_NotHaltFrei;

    GVL\_IO.DI\_SchutzhaubeZu       := GVL\_Simulation.SIM\_SchutzhaubeZu;

    GVL\_IO.DI\_PaletteEingang      := GVL\_Simulation.SIM\_PaletteEingang;

    GVL\_IO.DI\_PalettePositioniert := GVL\_Simulation.SIM\_PalettePositioniert;

    GVL\_IO.DI\_HubUnten            := GVL\_Simulation.SIM\_HubUnten;

    GVL\_IO.DI\_HubOben             := GVL\_Simulation.SIM\_HubOben;

    GVL\_IO.DI\_KollisionFrei       := GVL\_Simulation.SIM\_KollisionFrei;

    GVL\_IO.DI\_WerkerQuittierung   := GVL\_Simulation.SIM\_WerkerQuittierung;

    GVL\_IO.AI\_FolienTemperatur    := GVL\_Simulation.SIM\_FolienTemperatur;

    GVL\_IO.AI\_MotorTemperatur     := GVL\_Simulation.SIM\_MotorTemperatur;

    GVL\_IO.AI\_AntriebStrom        := GVL\_Simulation.SIM\_AntriebStrom;

END\_IF

// Wenn SIM\_Aktiv \= FALSE → GVL\_IO liest direkt von Hardware (AT %I\*)

**Hinweis:** `SIM_Aktiv` ist in GVL\_IO deklariert — im Online-Modus per Write Value auf TRUE setzen um Simulation zu aktivieren.

---

## 5\. PRG\_Palette — Schrittfolge S0–S9

### Variablen-Deklaration (PROGRAM MAIN)

Lokale `b`\-Variablen dienen als einheitliche Eingangssignale — egal ob Simulation oder Hardware. Die CASE-Struktur liest nur noch die `b`\-Variablen, nie direkt `GVL_IO.DI_*`.

PROGRAM MAIN

VAR

    tProzesszeit : TON;

    bNotHaltFrei         : BOOL;

    bSchutzhaubeZu       : BOOL;

    bPaletteEingang      : BOOL;

    bPalettePositioniert : BOOL;

    bHubUnten            : BOOL;

    bHubOben             : BOOL;

    bKollisionFrei       : BOOL;

    bWerkerQuittierung   : BOOL;

END\_VAR

### Implementierung (aktueller Stand)

// \=====================================================

// AUSGÄNGE ZU BEGINN JEDES SPS-ZYKLUS ZURÜCKSETZEN

// Verhindert ungewolltes "Hängenbleiben" von Ausgängen

// \=====================================================

GVL\_IO.DO\_SignalGruen      := FALSE;

GVL\_IO.DO\_AlarmRot         := FALSE;

GVL\_IO.DO\_Foerderband      := FALSE;

GVL\_IO.DO\_FoerderbandStopp := FALSE;

GVL\_IO.DO\_HubwerkHoch      := FALSE;

GVL\_IO.DO\_HubwerkRunter    := FALSE;

GVL\_IO.DO\_ProzessFreigabe  := FALSE;

GVL\_IO.DO\_Heizfreigabe     := FALSE;

// \=====================================================

// UMSCHALTUNG SIMULATION / HARDWARE

// SIM\_Aktiv \= TRUE  → b-Variablen aus GVL\_Simulation

// SIM\_Aktiv \= FALSE → b-Variablen direkt von Hardware

// \=====================================================

IF GVL\_IO.SIM\_Aktiv THEN

    bNotHaltFrei         := GVL\_Simulation.SIM\_NotHaltFrei;

    bSchutzhaubeZu       := GVL\_Simulation.SIM\_SchutzhaubeZu;

    bPaletteEingang      := GVL\_Simulation.SIM\_PaletteEingang;

    bPalettePositioniert := GVL\_Simulation.SIM\_PalettePositioniert;

    bHubUnten            := GVL\_Simulation.SIM\_HubUnten;

    bHubOben             := GVL\_Simulation.SIM\_HubOben;

    bKollisionFrei       := GVL\_Simulation.SIM\_KollisionFrei;

    bWerkerQuittierung   := GVL\_Simulation.SIM\_WerkerQuittierung;

ELSE

    bNotHaltFrei         := GVL\_IO.DI\_NotHaltFrei;

    bSchutzhaubeZu       := GVL\_IO.DI\_SchutzhaubeZu;

    bPaletteEingang      := GVL\_IO.DI\_PaletteEingang;

    bPalettePositioniert := GVL\_IO.DI\_PalettePositioniert;

    bHubUnten            := GVL\_IO.DI\_HubUnten;

    bHubOben             := GVL\_IO.DI\_HubOben;

    bKollisionFrei       := GVL\_IO.DI\_KollisionFrei;

    bWerkerQuittierung   := GVL\_IO.DI\_WerkerQuittierung;

END\_IF

// \=====================================================

// SCHRITTKETTE

// Ablauf: lesen → ausführen → E/A schreiben → von vorne

// Analoge Werte (AI\_\*) schreibt C\# per ADS direkt in

// GVL\_IO — kein Sim/HW-Unterschied nötig

// \=====================================================

CASE GVL\_IO.Schritt OF

    0: // S0 \- Anlage bereit und wartet auf Palette

        GVL\_IO.DO\_SignalGruen := TRUE;

        IF bNotHaltFrei

           AND bSchutzhaubeZu

           AND bHubUnten

           AND bPaletteEingang THEN

            GVL\_IO.Schritt := 1;

        END\_IF

    1: // S1 \- Förderband transportiert Palette

        GVL\_IO.DO\_Foerderband := TRUE;

        IF NOT bNotHaltFrei THEN

            GVL\_IO.Schritt := 8;

        ELSIF bPalettePositioniert THEN

            GVL\_IO.Schritt := 2;

        END\_IF

    2: // S2 \- Palette stoppen

        GVL\_IO.DO\_FoerderbandStopp := TRUE;

        GVL\_IO.Schritt := 3;   // Im nächsten SPS-Zyklus zu Hubwerk hoch

    3: // S3 \- Hubwerk fährt nach oben

        GVL\_IO.DO\_FoerderbandStopp := TRUE;

        GVL\_IO.DO\_HubwerkHoch      := TRUE;

        IF NOT bNotHaltFrei

           OR NOT bKollisionFrei THEN

            GVL\_IO.Schritt := 8;

        ELSIF bHubOben THEN

            GVL\_IO.Schritt := 4;

        END\_IF

    4: // S4 \- Temperatur prüfen

        GVL\_IO.DO\_FoerderbandStopp := TRUE;

        GVL\_IO.DO\_Heizfreigabe     := TRUE;

        IF GVL\_IO.AI\_MotorTemperatur \> 100.0 THEN

            GVL\_IO.Schritt := 8;

        ELSIF GVL\_IO.AI\_FolienTemperatur \>= 165.0

           AND GVL\_IO.AI\_FolienTemperatur \<= 185.0 THEN

            GVL\_IO.Schritt := 5;

        ELSE

            GVL\_IO.Schritt := 8;

        END\_IF

    5: // S5 \- Bearbeitung läuft fünf Sekunden

        GVL\_IO.DO\_FoerderbandStopp := TRUE;

        GVL\_IO.DO\_Heizfreigabe     := TRUE;

        GVL\_IO.DO\_ProzessFreigabe  := TRUE;

        tProzesszeit(IN := TRUE, PT := T\#5S);

        IF GVL\_IO.AI\_FolienTemperatur \< 165.0

           OR GVL\_IO.AI\_FolienTemperatur \> 185.0

           OR GVL\_IO.AI\_MotorTemperatur \> 100.0

           OR NOT bNotHaltFrei THEN

            tProzesszeit(IN := FALSE);

            GVL\_IO.Schritt := 8;

        ELSIF tProzesszeit.Q THEN

            tProzesszeit(IN := FALSE);

            GVL\_IO.Schritt := 6;

        END\_IF

    6: // S6 \- Hubwerk fährt nach unten

        GVL\_IO.DO\_FoerderbandStopp := TRUE;

        GVL\_IO.DO\_HubwerkRunter    := TRUE;

        IF NOT bNotHaltFrei THEN

            GVL\_IO.Schritt := 8;

        ELSIF bHubUnten THEN

            GVL\_IO.Schritt := 7;

        END\_IF

    7: // S7 \- Palette verlässt Station

        GVL\_IO.DO\_Foerderband := TRUE;

        IF NOT bPalettePositioniert THEN

            GVL\_IO.Schritt := 0;

        END\_IF

    8: // S8 \- Alarm setzen

        GVL\_IO.AlarmAktiv := TRUE;

        GVL\_IO.Schritt := 9;

    9: // S9 \- Warten auf Fehlerbeseitigung und Quittierung

        GVL\_IO.DO\_AlarmRot := TRUE;

        IF bWerkerQuittierung

           AND bNotHaltFrei

           AND GVL\_IO.AI\_FolienTemperatur \>= 165.0

           AND GVL\_IO.AI\_FolienTemperatur \<= 185.0

           AND GVL\_IO.AI\_MotorTemperatur \<= 100.0 THEN

            GVL\_IO.AlarmAktiv := FALSE;

            GVL\_IO.Schritt := 0;

        END\_IF

    ELSE // Unbekannter Schrittwert — Sicherheits-Fallback

        GVL\_IO.AlarmAktiv := TRUE;

        GVL\_IO.Schritt := 9;

END\_CASE

---

## 6\. Programm als Task registrieren und aktivieren

### PRG\_Palette in MainTask eintragen

SYSTEM

└── Tasks

    └── MainTask

        └── Doppelklick → Tab "POUs"

            → Add → PRG\_Palette auswählen

            → OK

### Konfiguration aktivieren und auf CP6606 laden

Build → Activate Configuration

→ "Restart TwinCAT System" bestätigen

→ Run Mode bestätigen

Danach im Solution Explorer: **grünes Icon** \= Runtime läuft ✅

### Variablen live beobachten

PRG\_Palette öffnen

→ Toolbar: "Login" (F11)

→ "Run" (F5)

→ Rechtsklick auf Variable → "Write Value" → Wert manuell setzen

---

## 7\. C\# ADS-Verbindung aufbauen

### NuGet-Paket installieren

Visual Studio → SensorAPI → NuGet-Pakete verwalten

→ Beckhoff.TwinCAT.Ads installieren

Kein TwinCAT-Install nötig — nur das NuGet-Paket reicht.

### AdsService.cs

using TwinCAT.Ads;

namespace SensorAPI;

public class AdsService : IDisposable

{

    private readonly AdsClient \_client;

    // CP6606 — NUR aus Beckhoff Configuration Tool ablesen, NIEMALS raten\!

    private const string AmsNetId \= "5.35.203.54.1.1";

    private const int AdsPort \= 851;   // Standard TwinCAT 3 SPS Port

    public AdsService()

    {

        \_client \= new AdsClient();

        \_client.Connect(AmsNetId, AdsPort);

    }

    public void SchreibeBool(string variablenName, bool wert)

    {

        var handle \= \_client.CreateVariableHandle($"GVL\_Simulation.{variablenName}");

        \_client.WriteAny(handle, wert);

        \_client.DeleteVariableHandle(handle);

    }

    public void SchreibeReal(string variablenName, float wert)

    {

        var handle \= \_client.CreateVariableHandle($"GVL\_Simulation.{variablenName}");

        \_client.WriteAny(handle, wert);

        \_client.DeleteVariableHandle(handle);

    }

    public int LeseSchritt()

    {

        var handle \= \_client.CreateVariableHandle("GVL\_IO.Schritt");

        var wert \= (short)\_client.ReadAny(handle, typeof(short));

        \_client.DeleteVariableHandle(handle);

        return wert;

    }

    public bool LeseBool(string variablenName)

    {

        var handle \= \_client.CreateVariableHandle($"GVL\_IO.{variablenName}");

        var wert \= (bool)\_client.ReadAny(handle, typeof(bool));

        \_client.DeleteVariableHandle(handle);

        return wert;

    }

    public float LeseReal(string variablenName)

    {

        var handle \= \_client.CreateVariableHandle($"GVL\_IO.{variablenName}");

        var wert \= (float)\_client.ReadAny(handle, typeof(float));

        \_client.DeleteVariableHandle(handle);

        return wert;

    }

    public void Dispose()

    {

        \_client.Disconnect();

        \_client.Dispose();

    }

}

**Wichtig:** C\# schreibt in `GVL_Simulation.*` — TwinCAT liest von dort und kopiert in GVL\_IO (wenn SIM\_Aktiv \= TRUE). Klare Trennung der Verantwortlichkeiten.

### In Program.cs registrieren

builder.Services.AddSingleton\<AdsService\>();

---

## 8\. C\# Simulationswerte berechnen und schreiben

### SimulationsService.cs

namespace SensorAPI;

public class SimulationsService

{

    private double \_zeitZaehler \= 0.0;

    private float \_motorTemp \= 25.0f;

    private float \_antriebStrom \= 0.0f;

    private readonly Random \_random \= new Random();

    public SimulationsSnapshot BerechneUndSchreibe(AdsService ads)

    {

        \_zeitZaehler \+= 0.05;

        // Folientemperatur: Sinus um Sollwert 175°C, Amplitude 12°C

        // → gelegentlich unter 165°C oder über 185°C \= Alarm wird ausgelöst

        float folienTemp \= 175.0f \+ (float)(12.0 \* Math.Sin(\_zeitZaehler \* 0.3));

        // Motortemperatur: steigt langsam, Abkühlzyklus bei 90°C

        \_motorTemp \= Math.Min(95.0f, \_motorTemp \+ 0.01f);

        if (\_motorTemp \> 90.0f)

            \_motorTemp \= 30.0f;

        // Antriebsstrom (EL3681): 0A im Stillstand, \~3A bei Betrieb

        bool foerderbandAktiv \= Math.Sin(\_zeitZaehler \* 0.07) \> 0.3;

        \_antriebStrom \= foerderbandAktiv

            ? 3.0f \+ (float)(\_random.NextDouble() \* 0.4 \- 0.2)   // 2.8–3.2A

            : 0.0f;

        // Simulierte DI-Bits

        bool hubUnten   \= Math.Sin(\_zeitZaehler \* 0.1) \> 0;

        bool hubOben    \= \!hubUnten;

        bool paletteEin \= foerderbandAktiv;

        // In GVL\_Simulation schreiben (TwinCAT liest von dort)

        try

        {

            ads.SchreibeReal("SIM\_FolienTemperatur", folienTemp);

            ads.SchreibeReal("SIM\_MotorTemperatur", \_motorTemp);

            ads.SchreibeReal("SIM\_AntriebStrom", \_antriebStrom);

            ads.SchreibeBool("SIM\_NotHaltFrei", true);

            ads.SchreibeBool("SIM\_SchutzhaubeZu", true);

            ads.SchreibeBool("SIM\_KollisionFrei", true);

            ads.SchreibeBool("SIM\_HubUnten", hubUnten);

            ads.SchreibeBool("SIM\_HubOben", hubOben);

            ads.SchreibeBool("SIM\_PaletteEingang", paletteEin);

            ads.SchreibeBool("SIM\_PalettePositioniert", paletteEin);

        }

        catch (Exception ex)

        {

            Console.WriteLine($"ADS Schreibfehler: {ex.Message}");

        }

        // Status aus GVL\_IO lesen (SPS-Ausgaben)

        int schritt \= 0;

        bool alarm \= false;

        bool signalGruen \= false;

        try

        {

            schritt     \= ads.LeseSchritt();

            alarm       \= ads.LeseBool("AlarmAktiv");

            signalGruen \= ads.LeseBool("DO\_SignalGruen");

        }

        catch (Exception ex)

        {

            Console.WriteLine($"ADS Lesefehler: {ex.Message}");

        }

        return new SimulationsSnapshot

        {

            FolienTemperatur \= folienTemp,

            MotorTemperatur  \= \_motorTemp,

            AntriebStrom     \= \_antriebStrom,

            Schritt          \= schritt,

            AlarmAktiv       \= alarm,

            SignalGruen      \= signalGruen

        };

    }

}

public class SimulationsSnapshot

{

    public float FolienTemperatur { get; set; }

    public float MotorTemperatur  { get; set; }

    public float AntriebStrom     { get; set; }

    public int   Schritt          { get; set; }

    public bool  AlarmAktiv       { get; set; }

    public bool  SignalGruen      { get; set; }

    // Grenzwert-Auswertung direkt im Modell

    public string TempStatus \=\> FolienTemperatur switch

    {

        \< 165.0f \=\> "ALARM",

        \> 185.0f \=\> "ALARM",

        \< 168.0f \=\> "WARNUNG",

        \> 182.0f \=\> "WARNUNG",

        \_         \=\> "OK"

    };

    public string MotorStatus \=\> MotorTemperatur switch

    {

        \> 100.0f \=\> "ALARM",

        \> 80.0f  \=\> "WARNUNG",

        \_         \=\> "OK"

    };

    public string StromStatus \=\> AntriebStrom switch

    {

        \> 6.0f  \=\> "ALARM",    // Überlast / Palette klemmt

        \> 4.5f  \=\> "WARNUNG",  // erhöhter Strom

        \_        \=\> "OK"

    };

}

---

## 9\. Dashboard erweitern

async function ladenPalette() {

    const response \= await fetch('/api/sensor/palette');

    const d \= await response.json();

    // Schritt S0-S9

    document.getElementById('schritt').textContent \= \`S${d.schritt}\`;

    // Folientemperatur mit Farbe

    const tempEl \= document.getElementById('folien-temp');

    tempEl.textContent \= \`${d.folienTemperatur.toFixed(1)} °C\`;

    tempEl.className \= d.tempStatus \=== 'ALARM'    ? 'alarm' :

                       d.tempStatus \=== 'WARNUNG'  ? 'warnung' : 'ok';

    // Motortemperatur

    const motorEl \= document.getElementById('motor-temp');

    motorEl.textContent \= \`${d.motorTemperatur.toFixed(1)} °C\`;

    motorEl.className \= d.motorStatus \=== 'ALARM'   ? 'alarm' :

                        d.motorStatus \=== 'WARNUNG' ? 'warnung' : 'ok';

    // Antriebsstrom (EL3681)

    const stromEl \= document.getElementById('antrieb-strom');

    stromEl.textContent \= \`${d.antriebStrom.toFixed(2)} A\`;

    stromEl.className \= d.stromStatus \=== 'ALARM'   ? 'alarm' :

                        d.stromStatus \=== 'WARNUNG' ? 'warnung' : 'ok';

    // Alarm-Banner

    document.getElementById('alarm-banner').style.display \=

        d.alarmAktiv ? 'block' : 'none';

}

setInterval(ladenPalette, 2000);

ladenPalette();

---

## 10\. Grenzwerte Referenz

### Folientemperatur (AI\_FolienTemperatur)

| Bereich | Wert | Reaktion |
| :---- | :---- | :---- |
| Sollwert | 175 °C | Regelziel |
| Warnzone | 168–182 °C | Prozess läuft, Vorwarnung |
| Prozessfenster | 165–185 °C | Freigabebedingung S4/S5 |
| Alarm kalt | \< 165 °C | Stopp \+ S8 |
| Alarm heiß | \> 185 °C | Sofortabschaltung \+ S8 |

### Motortemperatur Hubwerk (AI\_MotorTemperatur, EL3204 PT100)

| Grenze | Wert | Reaktion |
| :---- | :---- | :---- |
| Warnung | 80 °C | Dashboard-Hinweis |
| Alarm / Stopp | 100 °C | S8 Alarm, Hubwerk Stop |

### Antriebsstrom Förderband (AI\_AntriebStrom, EL3681)

| Bereich | Wert | Bedeutung |
| :---- | :---- | :---- |
| Normalbetrieb | 2.5–3.5 A | Förderband läuft mit Palette |
| Warnung | \> 4.5 A | erhöhte Last |
| Alarm | \> 6.0 A | Überlast / Palette klemmt → S8 |
| Leerlauf | \~0 A | Förderband steht |

---

## 11\. Typische Fehler TwinCAT & ADS

### TwinCAT kompiliert nicht — "Identifier expected"

**Problem:** Umlaut im Projektnamen (`TestFürWitron`). **Lösung:** Projekt umbenennen, keine Umlaute in Bezeichnern.

### ADS Timeout / Error 1861

**Problem:** AMS Net ID falsch (z.B. `192.168.1.10.1.1` statt `5.35.203.54.1.1`). **Lösung:** AMS Net ID NUR aus Beckhoff Configuration Tool ablesen — niemals ableiten\!

### EtherCAT-Klemmen nicht gefunden

**Problem:** Ethernet-Kabel steckt im EK1100 statt im X1-Port des CP6606. **Lösung:** Kabel umstecken: CP6606 X1 (IN) → EK1100 IN-Port.

### TwinCAT XAE "frisst" Netzwerkadapter

**Problem:** Realtime-Driver belegt den VM-Netzwerkadapter bei jedem Neustart. **Lösung:** Nativen Windows-PC verwenden — kein VM für TwinCAT XAE.

### PRG\_Palette läuft nicht

**Problem:** POU nicht in MainTask eingetragen. **Lösung:** SYSTEM → Tasks → MainTask → POUs → PRG\_Palette hinzufügen.

### C\# schreibt, TwinCAT reagiert nicht

**Problem:** SPS ist im Config Mode, nicht im Run Mode. **Lösung:** Build → Activate Configuration → Run Mode bestätigen.

### AdsServerException: Cannot connect Server 'AdsClient'

**Problem:** TwinCAT ADS Router läuft nicht auf dem PC wo die C\# API startet. **Ursache:** API lief in Parallels VM — kein ADS-Router, kein Netz zum CP6606. **Lösung:**

1. Visual Studio auf nativem Windows installieren (Community 2022, Workload: ASP.NET)  
2. Projekt von GitHub klonen: `git clone https://github.com/florian-englmeier/witron-prep.git`  
3. TwinCAT Static Routes prüfen: AMS Net ID `5.35.203.54.1.1` muss `Connected: x` zeigen  
4. API auf nativem Windows starten → ADS-Route direkt verfügbar

### TwinCAT Static Routes — Route prüfen

Windows Taskleiste → TwinCAT Icon → Rechtsklick → Router → Edit Routes

→ CP-23CB36 | Connected: x | AmsNetId: 5.35.203.54.1.1 | TCP\_IP

Wenn `Connected` leer ist → CP6606 nicht erreichbar → Netzwerk/Kabel prüfen. **Problem:** Attribut `qualified_only` erfordert vollen Präfix. **Lösung:** Variablenname in C\# als `GVL_Simulation.SIM_FolienTemperatur` angeben, nicht nur `SIM_FolienTemperatur`.

### AI\_\* Werte bleiben 0 obwohl GVL\_Simulation Werte hat

**Problem:** `AI_FolienTemperatur AT %I*` — TwinCAT überschreibt den Wert jedes SPS-Zyklus mit dem Hardware-Rohwert (0 wenn keine Klemme angeschlossen). **Lösung:** `AT %I*` bei allen analogen Eingängen entfernen die per Simulation oder ADS beschrieben werden. Nur digitale Ein-/Ausgänge behalten `AT %I*` / `AT %Q*`.

### EL3681 meldet "INIT to PREOP failed"

**Problem:** Klemme physisch neu gesteckt aber TwinCAT-Konfiguration kennt sie noch nicht. **Lösung:** E/A → Geräte → Gerät 1 (EtherCAT) → Rechtsklick → "Scan Boxes" → Änderungen übernehmen → Aktiviere Konfiguration.

### SIM\_Aktiv versehentlich auf FALSE gesetzt → sofort Alarm

**Problem:** Im Watch-Fenster versehentlich `SIM_Aktiv` statt `SIM_PaletteEingang` auf FALSE gesetzt → MAIN schaltet auf Hardware → alle DI\_\* \= FALSE → NotHalt FALSE → S8 Alarm. **Lösung:** Immer in GVL\_Simulation schreiben, nie direkt in GVL\_IO.DI\_\*\! Beim Schreiben Variablenname genau lesen bevor Enter gedrückt wird.

### Schritte S4/S5/S6 laufen zu schnell durch

**Problem:** S4 prüft Temperatur (175°C \= sofort OK), S5 Timer 5s läuft, S6 springt sofort weiter weil SIM\_HubUnten noch TRUE vom vorherigen Schritt. **Lösung:** Vor S3→S4 `SIM_HubUnten = FALSE` setzen — dann wartet S6 auf explizites `SIM_HubUnten = TRUE`. Timer in MAIN \[Online\] bei `tProzesszeit` beobachten — `ET` läuft von T\#0ms bis T\#5s.

---

## Testablauf — Manueller Durchlauf S0–S9

Reihenfolge für sauberen kontrollierten Test:

| Schritt | Aktion in GVL\_Simulation | Erwartetes Ergebnis |
| :---- | :---- | :---- |
| Start | SIM\_Aktiv (GVL\_IO) \= TRUE | DO\_SignalGruen \= TRUE |
| S0→S1 | SIM\_PaletteEingang \= TRUE | DO\_Foerderband \= TRUE |
| S1→S2 | SIM\_PalettePositioniert \= TRUE | DO\_FoerderbandStopp \= TRUE |
| S2→S3 | — automatisch | DO\_HubwerkHoch \= TRUE |
| S3→S4 | SIM\_HubOben \= TRUE, SIM\_HubUnten \= FALSE | DO\_Heizfreigabe \= TRUE |
| S4→S5 | — automatisch (Temp 175°C OK) | DO\_ProzessFreigabe \= TRUE |
| S5→S6 | — nach 5 Sekunden | DO\_HubwerkRunter \= TRUE |
| S6→S7 | SIM\_HubUnten \= TRUE, SIM\_HubOben \= FALSE | DO\_Foerderband \= TRUE |
| S7→S0 | SIM\_PalettePositioniert \= FALSE, SIM\_PaletteEingang \= FALSE | Schritt \= 0 |

**Status: Vollständig getestet und verifiziert ✅ — 08.07.2026**

---

## Versionsstand

| Version | Datum | Inhalt |
| :---- | :---- | :---- |
| v0.9.0 | 09.06.2026 | Alarm-Quittierung \+ Auto-Refresh |
| v1.0.0 | 03.07.2026 | TwinCAT Projekt, GVL\_IO, GVL\_Simulation, PRG\_Palette S0-S9 |
| v1.0.1 | 03.07.2026 | EL3681 ergänzt (AI\_AntriebStrom), GVL\_Simulation analoge Werte |
| v1.0.2 | 04.07.2026 | b-Variablen \+ Sim/HW-Umschaltung in PROGRAM MAIN, DOModi\_ Tippfehler korrigiert |
| v1.0.3 | 04.07.2026 | AT %I\* bei AI\_\* entfernt, SIM\_Aktiv in GVL\_IO, EL3681 live getestet, S0→S3 durchgelaufen |
| v1.0.4 | 08.07.2026 | Kompletter Zyklus S0→S1→S2→S3→S4→S5→S6→S7→S0 erfolgreich getestet — TwinCAT Programm verifiziert ✅ |
| v1.1.0 | 08.07.2026 | AdsService.cs \+ PaletteStatus Endpoint in SensorController, Singleton in Program.cs, Beckhoff.TwinCAT.Ads v7.0.172 |
| v1.1.1 | — | ADS-Verbindung live testen, API GET /palette gegen CP6606 |
| v1.2.0 | — | Dashboard Schrittanzeige \+ alle Messwerte |

---

*"Der Leitrechner schreibt, die SPS denkt, das Dashboard zeigt — und Jackie erklärt warum."* — Flo & Jackie, Juli 2026  
