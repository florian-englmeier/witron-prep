# TwinCAT-3-Lagerlogistikprojekt

## 1. Projektziel

Ziel ist der Aufbau einer realitätsnahen Lagerlogistik-Simulation mit TwinCAT 3 und vorhandener Beckhoff-Hardware.

Das System soll typische Funktionen einer automatisierten Förder- und Lageranlage abbilden:

- Erkennen einer Palette über digitale Sensoren
- Ansteuern von Fördertechnik und Hubeinheiten
- Abbilden eines definierten Prozessablaufs
- Überwachen von Temperaturen und elektrischen Zuständen
- Erzeugen und Quittieren von Alarmen
- Datenaustausch mit einer in C# entwickelten Benutzeroberfläche
- Spätere Erweiterung um Visualisierung, Diagnose und Messdatenauswertung

---

## 2. Grundidee der Anlage

Eine Palette wird durch eine kleine simulierte Förder- oder Lageranlage transportiert.

Dabei werden verschiedene Stationen durchlaufen:

1. Palette wird eingelegt.
2. Ein Sensor erkennt die Palette.
3. Der Förderantrieb wird freigegeben.
4. Die Palette fährt zur nächsten Station.
5. Ein weiterer Sensor bestätigt die Position.
6. Eine Hubeinheit oder ein anderer Aktor wird angesteuert.
7. Prozesswerte wie Temperatur, Spannung oder Strom werden überwacht.
8. Bei Grenzwertverletzungen wird ein Alarm erzeugt.
9. Ein Werker quittiert den Alarm über die Bedienoberfläche.
10. Der Prozess wird fortgesetzt oder in einen sicheren Zustand gebracht.

Die reale Mechanik kann zunächst vollständig durch SPS-Variablen simuliert werden.

---

## 3. Vorhandene und installierte Komponenten

### 3.1 Installierte Steuerungs- und Bedienhardware

- Beckhoff CP6606 Panel-PC beziehungsweise Bedienpanel
- TwinCAT 3 Engineering
- EtherCAT-Koppler
- EtherCAT-Endklemme
- DIN-Tragschiene TS35
- 24-V-DC-Netzteil
- Entwicklungsrechner mit Visual Studio
- C#-Benutzeroberfläche

### 3.2 Digitale Eingangsklemmen

Digitale Eingangsklemmen werden für simulierte oder reale Sensorsignale verwendet.

Mögliche Signale:

- Palette am Eingang vorhanden
- Palette an Station 1
- Palette an Station 2
- Hubwerk oben
- Hubwerk unten
- Schutztür geschlossen
- Not-Halt freigegeben
- Störung quittiert

Beispiel:

- EL1002 als zweikanalige digitale Eingangsklemme

Ein Eingang wird aktiv, wenn gegenüber dem gemeinsamen 0-V-Potential ein gültiges 24-V-Signal anliegt.

### 3.3 Digitale Ausgangsklemmen

Digitale Ausgänge können folgende Aktoren oder Signallampen simulieren beziehungsweise ansteuern:

- Fördermotor
- Hubeinheit aufwärts
- Hubeinheit abwärts
- Stopper öffnen
- Stopper schließen
- Warnleuchte
- Summer
- Betriebsanzeige

Reale induktive Lasten dürfen nicht ohne geeignete Schutzbeschaltung und Leistungsanpassung direkt angeschlossen werden.

### 3.4 Analoge Ein- und Ausgänge

Analoge Kanäle können für folgende Werte genutzt werden:

- Temperatur einer Druck- oder Folienstation
- Druck
- Weg
- Geschwindigkeit
- Sollwert eines Antriebs
- Belastung oder Prozesssignal

Bei der Inbetriebnahme müssen Signalart und Messbereich geprüft werden, zum Beispiel:

- 0 bis 10 V
- ±10 V
- 0 bis 20 mA
- 4 bis 20 mA
- Widerstandssensor oder Thermoelement

---

## 4. EL3681 als sinnvoller Projektbaustein

Die **Beckhoff EL3681** soll im Projekt als möglicher Baustein für die elektrische Zustandsüberwachung berücksichtigt werden.

> Frühere Bezeichnung im Gespräch: EL6381.  
> Gemeint ist sehr wahrscheinlich die EL3681.

### Mögliche Aufgaben im Projekt

- Messen einer Gleich- oder Wechselspannung
- Überwachen einer Versorgungsspannung
- Erfassen eines Stromwerts über einen geeigneten Messaufbau
- Erkennen von Unterspannung oder Überspannung
- Erkennen ungewöhnlicher elektrischer Lastzustände
- Diagnose eines simulierten Motors oder Verbrauchers
- Erzeugen von Warnungen und Alarmen
- Aufzeichnen elektrischer Messwerte für spätere Analysen

### Beispielhafte Alarmbedingungen

```text
Versorgungsspannung zu niedrig
Versorgungsspannung zu hoch
Motorstrom zu hoch
Motorstrom trotz eingeschaltetem Ausgang zu niedrig
Elektrischer Verbraucher nicht angeschlossen
Unplausibler Messwert
```

### Beispiel für eine Diagnose

Wenn der Fördermotor eingeschaltet ist, wird ein bestimmter Strombereich erwartet.

```text
Motor AUS:
Strom ungefähr 0 A

Motor EIN:
Strom innerhalb eines definierten Normalbereichs

Motor EIN und Strom zu niedrig:
Möglicherweise Leitungsbruch oder Motor nicht angeschlossen

Motor EIN und Strom zu hoch:
Möglicherweise Blockade oder Überlast
```

Die EL3681 kann damit später ein gutes Bindeglied zwischen SPS-Programmierung, Messtechnik, Zustandsüberwachung und Datenanalyse bilden.

---

## 5. Elektrischer Grundaufbau

### 5.1 Versorgung

Für den Aufbau wird eine 24-V-DC-Versorgung verwendet.

Wichtig:

- `+24 V` ist das positive Versorgungspotential.
- `0 V` ist das Bezugspotential.
- `0 V` ist nicht automatisch Schutzleiter.
- Schutzleiter, Funktionserde und 0 V müssen entsprechend dem Aufbau korrekt behandelt werden.
- Vor dem Anschließen einer Klemme muss deren Datenblatt geprüft werden.

### 5.2 Digitale Eingangssimulation

Ein digitaler Eingang kann für Testzwecke mit 24 V beschaltet werden.

Voraussetzung:

- Das 0-V-Potential der Signalquelle und der Eingangsklemme muss gemeinsam verbunden sein.
- Die zulässige Eingangsspannung darf nicht überschritten werden.
- Es muss eindeutig bekannt sein, welcher Anschluss Signal und welcher Anschluss Versorgung beziehungsweise Masse ist.

Für erste Softwaretests können Eingangsvariablen auch intern simuliert werden. Das direkte Schreiben eines physisch verknüpften Eingangssignals ist jedoch nicht immer möglich, weil das Prozessabbild bei jedem Buszyklus mit dem realen Eingangswert überschrieben wird.

---

## 6. TwinCAT-Projektstruktur

Empfohlene Struktur:

```text
PLC
├── DUTs
│   ├── E_Anlagenzustand
│   ├── E_Prozessschritt
│   ├── ST_Eingaenge
│   ├── ST_Ausgaenge
│   ├── ST_Analogwerte
│   └── ST_Alarm
│
├── GVLs
│   ├── GVL_IO
│   ├── GVL_Process
│   ├── GVL_Alarm
│   └── GVL_HMI
│
├── POUs
│   ├── MAIN
│   ├── FB_Lagerprozess
│   ├── FB_AlarmManager
│   ├── FB_Simulation
│   └── FB_Diagnose
│
└── VISUs
    └── optionale TwinCAT-Visualisierung
```

---

## 7. Globale Variablen

### 7.1 Physische beziehungsweise simulierte Eingänge

```iecst
VAR_GLOBAL
    bNotHaltFrei        : BOOL;
    bPaletteEingang     : BOOL;
    bPaletteStation1    : BOOL;
    bPaletteStation2    : BOOL;
    bHubOben            : BOOL;
    bHubUnten           : BOOL;
    bQuittierung        : BOOL;
END_VAR
```

### 7.2 Ausgänge

```iecst
VAR_GLOBAL
    bFoerdererEin       : BOOL;
    bHubAuf             : BOOL;
    bHubAb              : BOOL;
    bWarnleuchte        : BOOL;
    bSummer             : BOOL;
END_VAR
```

### 7.3 Prozesswerte

```iecst
VAR_GLOBAL
    rTemperatur         : REAL;
    rSpannung           : REAL;
    rStrom              : REAL;
    rTemperaturGrenze   : REAL := 80.0;
    rSpannungMin        : REAL := 22.0;
    rSpannungMax        : REAL := 26.0;
END_VAR
```

### 7.4 Status und Alarm

```iecst
VAR_GLOBAL
    bAnlageBereit       : BOOL;
    bAutomatikAktiv     : BOOL;
    bStoerung           : BOOL;
    bAlarmTemperatur    : BOOL;
    bAlarmUnterspannung : BOOL;
    bAlarmUeberspannung : BOOL;
    nProzessschritt     : INT;
    sStatusText         : STRING(255);
    sAlarmText          : STRING(255);
END_VAR
```

---

## 8. Trennung zwischen realen und simulierten Signalen

Für die Entwicklung ist eine Umschaltung zwischen Hardwarebetrieb und Simulation sinnvoll.

```iecst
VAR_GLOBAL
    bSimulationAktiv : BOOL := TRUE;
END_VAR
```

Beispiel:

```iecst
IF bSimulationAktiv THEN
    bPaletteEingangIntern := bPaletteEingangSimulation;
ELSE
    bPaletteEingangIntern := bPaletteEingangHardware;
END_IF
```

Vorteile:

- SPS-Logik kann ohne angeschlossene Sensoren getestet werden.
- Physische Eingänge werden nicht ständig manuell überschrieben.
- Die C#-Oberfläche kann gezielt Sensorsignale simulieren.
- Später kann auf reale Hardware umgeschaltet werden.

---

## 9. Erster einfacher Prozessablauf

Ein übersichtlicher Ablauf kann zunächst mit einer Schrittkette umgesetzt werden.

```iecst
CASE nProzessschritt OF

    0:
        sStatusText := 'Anlage wartet';
        bFoerdererEin := FALSE;
        bHubAuf := FALSE;
        bHubAb := FALSE;

        IF bNotHaltFrei AND bPaletteEingang THEN
            nProzessschritt := 10;
        END_IF

    10:
        sStatusText := 'Palette wird transportiert';
        bFoerdererEin := TRUE;

        IF bPaletteStation1 THEN
            bFoerdererEin := FALSE;
            nProzessschritt := 20;
        END_IF

    20:
        sStatusText := 'Hub fährt nach oben';
        bHubAuf := TRUE;

        IF bHubOben THEN
            bHubAuf := FALSE;
            nProzessschritt := 30;
        END_IF

    30:
        sStatusText := 'Prozess abgeschlossen';

        IF NOT bPaletteEingang THEN
            nProzessschritt := 0;
        END_IF

ELSE
    nProzessschritt := 0;

END_CASE
```

---

## 10. Alarmüberwachung

Beispielhafte Alarmbedingungen:

```iecst
bAlarmTemperatur := rTemperatur > rTemperaturGrenze;
bAlarmUnterspannung := rSpannung < rSpannungMin;
bAlarmUeberspannung := rSpannung > rSpannungMax;

bStoerung :=
    bAlarmTemperatur
    OR bAlarmUnterspannung
    OR bAlarmUeberspannung;
```

Ausgabe eines Alarmtexts:

```iecst
IF bAlarmTemperatur THEN
    sAlarmText := 'Temperaturgrenze überschritten';

ELSIF bAlarmUnterspannung THEN
    sAlarmText := 'Versorgungsspannung zu niedrig';

ELSIF bAlarmUeberspannung THEN
    sAlarmText := 'Versorgungsspannung zu hoch';

ELSE
    sAlarmText := '';
END_IF
```

Die Quittierung sollte einen anstehenden physikalischen Fehler nicht einfach löschen. Erst wenn die Fehlerursache nicht mehr vorhanden ist, darf der Alarm zurückgesetzt werden.

---

## 11. Verbindung zur C#-Benutzeroberfläche

Die vorhandene Visual-Studio-Anwendung soll als Bedien- und Diagnoseschnittstelle erhalten bleiben.

Mögliche Kommunikation:

- Beckhoff ADS
- ADS.NET
- Lesen und Schreiben von SPS-Variablen
- Anzeige von Zuständen und Prozessschritten
- Simulation von Sensoren
- Eingabe von Grenzwerten
- Alarmanzeige und Quittierung
- Protokollierung von Ereignissen
- Spätere Speicherung in einer Datenbank

### Mögliche HMI-Elemente

- Start
- Stopp
- Reset
- Automatikbetrieb
- Simulationsbetrieb
- Palette einlegen
- Sensor Station 1
- Sensor Station 2
- Hub oben
- Hub unten
- Temperaturvorgabe
- Spannungsanzeige
- Stromanzeige
- Alarmübersicht
- Quittiertaste
- Prozessschrittanzeige
- Statusprotokoll

---

## 12. Verknüpfung der Hardware mit SPS-Variablen

Die EtherCAT-Kanäle werden im TwinCAT-I/O-Baum mit SPS-Variablen verknüpft.

Beispiel:

```text
EL1002 Kanal 1
    ↔ GVL_IO.bPaletteEingangHardware

Digitaler Ausgang Kanal 1
    ↔ GVL_IO.bFoerdererEinHardware
```

Eine Verknüpfung kann über den jeweiligen Prozessdatenkanal im I/O-Baum hergestellt oder wieder entfernt werden.

Beim Testen ist zu beachten:

- Physische Eingänge werden durch die Hardware aktualisiert.
- Manuell geschriebene Werte können deshalb sofort wieder überschrieben werden.
- Für Tests ist eine getrennte Simulationsvariable sauberer.
- Ausgänge dürfen nur geschaltet werden, wenn die angeschlossene Last dafür geeignet ist.

---

## 13. Aktueller Arbeitsstand

- TwinCAT-Projekt ist angelegt.
- Globale Variablen und Datentypen wurden begonnen.
- `MAIN` ist vorhanden.
- Erste Hardwareverknüpfungen wurden getestet.
- SPS-Abfrage und Buskommunikation laufen.
- Digitale Eingänge können grundsätzlich eingelesen werden.
- Das Setzen von Online-Werten wurde getestet.
- Die Unterscheidung zwischen physischem Eingang und Simulationswert muss sauber umgesetzt werden.
- Die C#- beziehungsweise Visual-Studio-Schnittstelle soll weiterverwendet werden.
- Die EL3681 wird als möglicher Baustein für Spannungs-, Strom- und Zustandsüberwachung vorgemerkt.

---

## 14. Empfohlene nächste Schritte

1. Hardwareliste mit exakten Klemmenbezeichnungen vervollständigen.
2. Alle Kanäle eindeutig dokumentieren.
3. `GVL_IO` in Hardware- und Simulationssignale aufteilen.
4. Umschaltung `bSimulationAktiv` einbauen.
5. Erste Schrittkette in `MAIN` oder `FB_Lagerprozess` implementieren.
6. Alarmmanager für Temperatur und elektrische Messwerte erstellen.
7. ADS-Verbindung zur C#-Oberfläche testen.
8. Statuswerte in der Oberfläche anzeigen.
9. Sensorsignale über die Oberfläche simulieren.
10. Später reale Sensoren, Aktoren und die EL3681 integrieren.

---

## 15. Langfristige Erweiterungen

- Zustandsautomat mit ENUM statt numerischer Schrittwerte
- Funktionsbausteine für Förderer, Hubwerk und Sensoren
- Zeitüberwachung einzelner Bewegungen
- Fehler bei nicht erreichtem Endschalter
- Motorstromüberwachung
- Predictive-Maintenance-Ansatz
- Datenbankanbindung
- Trendanzeige für Temperatur, Spannung und Strom
- Protokollierung von Alarmen und Quittierungen
- Benutzer- und Rollenverwaltung
- Digitaler Zwilling der Anlage
- Automatisierte Tests der SPS-Logik
- Git-Repository für SPS- und C#-Projekt
- Dokumentation der Ein- und Ausgangsbelegung

---

## 16. Strategische Roadmap

Die weitere Entwicklung sollte bewusst in klar getrennten Stufen erfolgen. Dadurch lassen sich Fehler leichter zuordnen und das Projekt bleibt übersichtlich.

### Phase 1: Hardware und Simulation sauber trennen

Für jedes relevante Eingangssignal werden drei Ebenen vorgesehen:

```iecst
bPaletteEingangHardware
bPaletteEingangSimulation
bPaletteEingang
```

Die SPS entscheidet abhängig vom Betriebsmodus, welches Signal verwendet wird:

```iecst
IF bSimulationAktiv THEN
    bPaletteEingang := bPaletteEingangSimulation;
ELSE
    bPaletteEingang := bPaletteEingangHardware;
END_IF
```

Diese Trennung ist der wichtigste nächste Schritt.

Vorteile:

- Der Prozess kann ohne reale Sensoren getestet werden.
- Physische Eingänge werden nicht manuell überschrieben.
- Die C#-Oberfläche kann gezielt Simulationswerte setzen.
- Hardwarefehler und Softwarefehler lassen sich besser unterscheiden.

### Phase 2: Einen kleinen vollständigen Prozess umsetzen

Zunächst wird nur ein kompakter, vollständig funktionierender Ablauf aufgebaut:

```text
Palette erkannt
→ Förderer startet
→ Palette erreicht Station
→ Förderer stoppt
→ Hub fährt hoch
→ Prozess abgeschlossen
```

Dieser Ablauf soll zuerst vollständig funktionieren:

- in der SPS
- im Online-Modus
- im Simulationsbetrieb
- später mit einem realen Eingang

Ein kleiner stabiler Ablauf ist strategisch wertvoller als viele gleichzeitig begonnene Funktionen.

### Phase 3: Zustandsmaschine einführen

Der Prozess sollte nicht dauerhaft nur über viele einzelne BOOL-Verknüpfungen gesteuert werden.

Empfohlen wird ein eigener Datentyp:

```iecst
TYPE E_ProcessState :
(
    Idle,
    Transport,
    Lifting,
    Complete,
    Fault
);
END_TYPE
```

Beispiel für die Verwendung:

```iecst
CASE eProcessState OF

    E_ProcessState.Idle:
        bFoerdererEin := FALSE;
        bHubAuf := FALSE;

        IF bNotHaltFrei AND bPaletteEingang THEN
            eProcessState := E_ProcessState.Transport;
        END_IF

    E_ProcessState.Transport:
        bFoerdererEin := TRUE;

        IF bPaletteStation1 THEN
            bFoerdererEin := FALSE;
            eProcessState := E_ProcessState.Lifting;
        END_IF

    E_ProcessState.Lifting:
        bHubAuf := TRUE;

        IF bHubOben THEN
            bHubAuf := FALSE;
            eProcessState := E_ProcessState.Complete;
        END_IF

    E_ProcessState.Complete:
        IF NOT bPaletteEingang THEN
            eProcessState := E_ProcessState.Idle;
        END_IF

    E_ProcessState.Fault:
        bFoerdererEin := FALSE;
        bHubAuf := FALSE;
        bHubAb := FALSE;

END_CASE
```

Damit wird der Ablauf klarer, leichter erweiterbar und besser in der HMI darstellbar.

### Phase 4: C#-Schnittstelle anbinden

Die Visual-Studio-Anwendung sollte erst dann erweitert werden, wenn der SPS-Prozess allein stabil läuft.

Für die erste Ausbaustufe reichen:

- Simulationsmodus ein und aus
- Palette simulieren
- Start
- Stopp
- Reset
- Prozesszustand anzeigen
- Alarmtext anzeigen
- Quittierung auslösen

Zunächst nicht erforderlich:

- aufwendige Diagramme
- Datenbankanbindung
- Benutzerverwaltung
- komplexe Statistik
- umfangreiche Animationen

### Phase 5: Reale Hardware schrittweise integrieren

Die reale Hardware wird kanalweise eingebunden.

Empfohlene Reihenfolge:

1. Einen digitalen Eingang auswählen.
2. Kanal im EtherCAT-Baum prüfen.
3. SPS-Variable verknüpfen.
4. Eingang elektrisch testen.
5. Wert im Online-Modus beobachten.
6. Hardware- und Simulationsbetrieb vergleichen.
7. Erst danach den nächsten Kanal integrieren.

Bei Ausgängen sollten zunächst geeignete Testlasten oder Signallampen verwendet werden.

Motoren, Ventile und größere induktive Lasten benötigen eine passende Leistungs- und Schutzbeschaltung.

### Phase 6: Alarm- und Diagnosefunktionen ergänzen

Wenn der Grundprozess läuft, werden Überwachungen ergänzt:

- Zeitüberschreitung beim Transport
- Endschalter nicht erreicht
- unplausible Sensorkombination
- Temperaturgrenzwert überschritten
- Unterspannung
- Überspannung
- Ausgang aktiv, aber keine erwartete Reaktion

Ein Alarm darf erst vollständig zurückgesetzt werden, wenn:

1. die Ursache nicht mehr ansteht und
2. der Bediener quittiert hat.

### Phase 7: EL3681 integrieren

Die EL3681 wird erst eingesetzt, wenn der Grundprozess und die Hardwareanbindung stabil sind.

Mögliche Diagnose:

```text
Fördermotor AUS
→ Strom ungefähr 0 A
```

```text
Fördermotor EIN
→ Strom innerhalb des erwarteten Bereichs
```

```text
Fördermotor EIN, aber Strom zu niedrig
→ möglicher Leitungsbruch oder Verbraucher nicht angeschlossen
```

```text
Fördermotor EIN und Strom zu hoch
→ mögliche Blockade oder Überlast
```

Damit wird das Projekt um echte messtechnische Zustandsüberwachung erweitert.

---

## 17. Priorisierte nächste Arbeitsschritte

Die konkret empfohlene Reihenfolge lautet:

1. `GVL_IO` in Hardware- und Simulationssignale aufteilen.
2. `bSimulationAktiv` einführen.
3. `E_ProcessState` als ENUM anlegen.
4. `FB_Lagerprozess` erstellen.
5. Ablauf `Idle → Transport → Lifting → Complete` umsetzen.
6. Prozess vollständig im Online-Modus testen.
7. C#-Oberfläche über ADS anbinden.
8. Einen realen digitalen Eingang integrieren.
9. Weitere Eingänge und Ausgänge ergänzen.
10. Alarmmanager aufbauen.
11. EL3681 für Spannungs-, Strom- und Zustandsüberwachung integrieren.
12. Datenaufzeichnung und spätere Analyse ergänzen.

### Strategische Leitlinie

Die Reihenfolge lautet bewusst:

```text
Simulation
→ stabiler SPS-Prozess
→ C#-Schnittstelle
→ reale Hardware
→ Diagnose
→ EL3681
→ Datenanalyse
```

Dadurch bleibt jede Projektphase einzeln testbar und nachvollziehbar.

