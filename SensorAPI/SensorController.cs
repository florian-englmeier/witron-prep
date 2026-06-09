using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.ComponentModel.DataAnnotations;

namespace SensorAPI
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorController : Controller
    {
        private static List<SensorData> _sensoren = new List<SensorData>
        {
            new SensorData { Name = "Ofen-1", Einheit = "°C", Min = -20, Max = 120 },
            new SensorData { Name = "Leitung-A", Einheit = "bar", Min = 120, Max = 10 },
            new SensorData { Name = "Feuchtigkeit", Einheit = "%", Min = 0, Max = 100 }

        };

        private static Random _random = new Random();

        // Get api/sensor - Alle Sensoren mit aktuellen Messwerten zurückgeben
        [HttpGet]
        public ActionResult<List<SensorData>> GetSensoren()
        {
            // Aktuelle Messwerte simulieren
            foreach (var s in _sensoren)
            {
                s.Messwert = Math.Round(_random.NextDouble() * (s.Max - s.Min) + s.Min, 2);
            }
            return Ok(_sensoren);
        }

        [HttpGet("alarm")]
        public ActionResult<List<SensorData>> GetAlarm()
        {
            // Sensoren mit Messwerten außerhalb des zulässigen Bereichs zurückgeben
            var alarme = _sensoren.Where(s => s.Messwert < s.Min || s.Messwert > s.Max).ToList();
            return Ok(alarme);

        }
    }



    public class SensorData
    {
        public string Name { get; set; } = "";
        public string Einheit { get; set; } = "";
        public double Messwert { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
    }
}