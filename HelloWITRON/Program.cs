
// Hauptprogramm
// === WITRON Sensor-Simulator v2 ===
// List<Sensor>, foreach, Vererbung, LINQ-Vorgeschmack

List<Sensor> sensoren = new List<Sensor>
{
    new TemperaturSensor("Ofen-1",-20,120),
    new DruckSensor("Leitung-A",0,10),
    new Sensor("Feuchtigkeit","%",0,100),
};

for (int i =0; i < 5; i++)
{
    Console.WriteLine($"----- Messzyklus {i++1} ----");

    foreach (Sensor sensor in sensoren)
    {
        s.Messsen();
        s.Anzeigen();
    }
    Thread.Sleep(500);
    Console.WriteLine();
}

class Sensor
{
    public string Name { get; set; }
    public string Einheit { get; set; }
    public double Messwert { get; private set; }
    private Random _random = new Random();
    public Sensor(string name, string einheit)
    {
        Name = name;
        Einheit = einheit;
    }
    public void Messen()
    {
        Messwert = Math.Round(_random.NextDouble() * 100, 2);
    }

    public void Anzeigen()
    {
        Console.WriteLine($"{Name}: {Messwert} {Einheit}");
    }
}