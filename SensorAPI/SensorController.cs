using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.ComponentModel.DataAnnotations;

namespace SensorAPI
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorController : Controller
    {
        private readonly AdsService _ads;

        public SensorController(AdsService ads)
        {
            _ads = ads;
        }

        private static List < SensorData > _sensoren = new List<SensorData>
        {
            new SensorData { Name = "Antrieb-1 Drehzahl",   Einheit = "RPM",  Min = 0,  Max = 1500, Gruppe = "Antriebe" },
            new SensorData { Name = "Antrieb-1 Schwingung", Einheit = "mm/s", Min = 0,  Max = 4.5,  Gruppe = "Antriebe" },
            new SensorData { Name = "Antrieb-2 Drehmoment", Einheit = "Nm",   Min = 0,  Max = 120,  Gruppe = "Antriebe" },
            new SensorData { Name = "Spannung Zone-A",      Einheit = "V",    Min = 22, Max = 26,   Gruppe = "Elektrik" },
            new SensorData { Name = "Lastbewegung Zone-A",  Einheit = "kg",   Min = 0,  Max = 500,  Gruppe = "Foerderband" },
            new SensorData { Name = "Lastbewegung Zone-B",  Einheit = "kg",   Min = 0,  Max = 500,  Gruppe = "Foerderband" },
            new SensorData { Name = "Betriebsstunden",      Einheit = "h",    Min = 0,  Max = 8760, Gruppe = "Wartung" },
        };


        private static Random _random = new Random();

        // Get api/sensor - Alle Sensoren mit aktuellen Messwerten zurückgeben
        [HttpGet]
        public ActionResult<List<SensorData>> GetSensoren()
        {
            foreach (var s in _sensoren)
            {
                // Aktuelle Messwerte simulieren
                // Neu — manchmal 10 % über die Grenzen:
                double spielraum = (s.Max - s.Min) * 0.1;
                s.Messwert = Math.Round(_random.NextDouble() * (s.Max - s.Min + 2 * spielraum) + s.Min - spielraum, 2);

            }
            return Ok(_sensoren);
        }
        
        // GET api/sensor/ads-status
        [HttpGet("ads-status")]
        public ActionResult GetAdsStatus()
        {
            return Ok(new
            {
                Connected = _ads.IsConnected,
                State = _ads.GetPlcState()
            });
        }

        // GET api/sensor/live - Live-Werte aus der PalettenStation-SPS
        [HttpGet("live")]
        public ActionResult GetLive()
        {
            var schritt = _ads.ReadInt("GVL_IO.Schritt");
            var folienTemp = _ads.ReadReal("GVL_IO.AI_FolienTemperatur");
            var motorTemp = _ads.ReadReal("GVL_IO.AI_MotorTemperatur");
            var alarm = _ads.ReadBool("GVL_IO.AlarmAktiv");

            return Ok(new
            {
                Connected = _ads.IsConnected,
                Schritt = schritt,
                SchrittText = SchrittZuText(schritt),
                FolienTemperatur = Math.Round(folienTemp, 1),
                MotorTemperatur = Math.Round(motorTemp, 1),
                AlarmAktiv = alarm,
                Zeitstempel = DateTime.Now.ToString("HH:mm:ss")
            });
        }

        // Hilfsmethode: übersetzt die Schrittnummer in verständlichen Text
        private string SchrittZuText(short schritt)
        {
            return schritt switch
            {
                0 => "S0 - Bereit, wartet auf Palette",
                1 => "S1 - Foerderband transportiert",
                2 => "S2 - Palette stoppen",
                3 => "S3 - Hubwerk faehrt hoch",
                4 => "S4 - Temperatur pruefen",
                5 => "S5 - Bearbeitung laeuft",
                6 => "S6 - Hubwerk faehrt runter",
                7 => "S7 - Palette verlaesst Station",
                8 => "S8 - ALARM gesetzt",
                9 => "S9 - Warten auf Quittierung",
                _ => $"Unbekannter Schritt: {schritt}"
            };
        }
        [HttpGet("alarm")]
        public ActionResult<List<SensorData>> GetAlarm()
        {
            // Sensoren mit Messwerten außerhalb des zulässigen Bereichs zurückgeben
            var alarme = _sensoren.Where(s => s.Messwert < s.Min || s.Messwert > s.Max).ToList();
            return Ok(alarme);

        }

        [HttpPost("quittieren")]
        public ActionResult Quittieren([FromBody] QuittierungRequest request)
        {
            var sensor = _sensoren.FirstOrDefault(s => s.Name == request.SensorName);
            if (sensor == null)
            {
                return NotFound($"Sensor {request.SensorName} nicht gefunden");
            }

            sensor.AlarmQuittiert = true;
            sensor.QuittiertVon = request.Techniker;
            sensor.QuittiertUm = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return Ok($"Alarm fuer {sensor.Name} quittiert von {request.Techniker}");
        }
    }

    public class SensorData
    {
        public string Name { get; set; } = "";
        public string Einheit { get; set; } = "";
        public double Messwert { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string Gruppe { get; set; } = "";  // NEU!
        public bool AlarmQuittiert { get; set; } = false;
        public string QuittiertVon { get; set; } = "";
        public string QuittiertUm { get; set; } = "";
    }

    public class QuittierungRequest
    {
        public string SensorName { get; set; } = "";
        public string Techniker { get; set; } = "";
    }

}



