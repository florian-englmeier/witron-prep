
// Hauptprogramm
Sensor temperatur = new Sensor("Temperatur", "Grad C");
Sensor druck = new Sensor("Druck", "bar");




for (int i = 0; i < 5; i++)
{
    Console.WriteLine($"--- Messzyklus {i + 1} ---");
    temperatur.Messen();
    temperatur.Anzeigen();
    druck.Messen();
    druck.Anzeigen();
    Thread.Sleep(500); // Pause von 500 Taktzyköus
    Console.WriteLine(); // Leerzeile für bessere Lesbarkeit
}

// Sensor Klasse -- eie eine Messgerät Abstraktion
class Sensor
{
    public string Name { get; set; }
    public string Einheit { get; set; }
    public double Messwert { get; private set; }

    private Random _random = new Random();

    // Konstruktor
    public Sensor(string name,string einheit)
    {
        Name = name;
        Einheit = einheit;
      
    }
    // Methode -- neuen Messwert generieren
    public void Messen()
    {
        Messwert = Math.Round(_random.NextDouble() * 100, 2); // Zufälligen Messwert zwischen 0 und 100 generieren
    }
    // Ausgabe des Messwerts
    public void Anzeigen()
    {
        Console.WriteLine($"{Name}: {Messwert} {Einheit}");
    }

}

