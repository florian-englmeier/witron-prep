using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;
using System;

public class SensorBenchmark
{
    private Sensor _temperatur;
    private Sensor _druck;
    [GlobalSetup]
    public void Setup()
    {
        _temperatur = new Sensor("Temperatur", "Grad C");
        _druck = new Sensor("Druck", "bar");
    }

    [Benchmark]
    public void MeasureTwoSensors()
    {
        _temperatur.Messen();
        _druck.Messen();
    }

    [Benchmark]
    public void MeasureAndDisplaySensors()
    {
        _temperatur.Messen();
        _temperatur.Anzeigen();
        _druck.Messen();
        _druck.Anzeigen();
    }
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