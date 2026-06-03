
// Hauptprogramm -- v3: Vererbung + Alarm
List<Sensor> sensoren = new List<Sensor>
{
   new TemperaturSensor("Ofen-1"),
   new DruckSensor("Leitung-A"),
   new Sensor("Feuchtigkeit-1", "%", 0, 100)
};


for (int i = 0; i < 5; i++)
{
    Console.WriteLine($"--- Messzyklus {i + 1} ---");

    foreach (Sensor s in sensoren)
    {
        s.Messen();
        Console.WriteLine(s);  // statt s.Anzeigen()
    }

    Thread.Sleep(500);
    Console.WriteLine();
}

Console.WriteLine("=== Alarm-Pruefung ===");
foreach (Sensor s in sensoren)
{
    if (s.IstAlarm())
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"ALARM: {s.Name} = {s.Messwert} {s.Einheit}");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"OK: {s.Name} = {s.Messwert} {s.Einheit}");
        Console.ResetColor();
    }
}

class Sensor // ------------------------------------------------ Sensor Baisklasse ------------------------
{
    public string Name { get; set; }
    public string Einheit { get; set; }
    public double Messwert { get; protected set; } // protected statt private!
    public double Min { get; set; }
    public double Max { get; set; }


    private Random _random = new Random();

    // Konstruktor
    public Sensor(string name, string einheit, double min, double max)
    {
        Name = name;
        Einheit = einheit;
        Min = min;
        Max = max;
    }

    // virtual = "darf von Kindlassen überschrieben werden"
    public virtual void Messen()
    {
        // Zufallswert generieren

        double spielraum = (Max - Min) * 0.1;
        Messwert = Math.Round(_random.NextDouble() * (Max - Min + 2 * spielraum) + Min - spielraum, 2);
    }
    public void Anzeigen()
    {
        Console.WriteLine($"{Name}: {Messwert:F2} {Einheit}");
    }

    // ToString -- wird automatisch aufgerufen bei Console.WriteLine(objekt)
    public override string ToString()
    {
        return $"  {Name,-15} {Messwert,8:F2} {Einheit}";

    }

    public bool IstAlarm()
    {
        return Messwert < Min || Messwert > Max;
    }

}
// Die Kindklasse braucht nur den Namen — Einheit, Min, Max sind fest eingebaut/
class TemperaturSensor : Sensor
{
        public TemperaturSensor(string name) : base(name, "Grad C", -20, 120)
    {
    }
    // override = "Ich überschreibe die Messen()-Methode der Elternklasse"
    public override void Messen()
    {
        base.Messen();  // erstmal normal messen wie der Eltern-Sensor
        // Dann Temperatur realistischer machen: auf ganze Grad runden
        Messwert = Math.Round(Messwert);
    }

}
class DruckSensor : Sensor
{
    public DruckSensor(string name) : base(name, "bar", 0, 10)
    {
    }
    public override void Messen()
    {
        base.Messen();
        // Druck auf 3 Nachkommastellen -- industrielle Präzision
        Messwert = Math.Round(Messwert, 3);
    }
    
 }

