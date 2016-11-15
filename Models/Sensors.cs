using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Events;

namespace SensorTagPi.Models
{
    public class SensorStatus
    {
        public Sensors Sensor { get; private set; }
        public bool    Active { get; private set; }

        public SensorStatus(Sensors sensor, bool active)
        {
            Sensor = sensor;
            Active = active;
        }
    };

    public class SensorStatusEvent : PubSubEvent<SensorStatus>
    {
    };

    public class TemperatureSensor
    {
        public double Temperature { get; set; }
        public double Ambient { get; set; }

        public TemperatureSensor( double temperature = double.NaN, double ambient = double.NaN)
        {
            Temperature  = temperature;
            Ambient      = ambient;
        }
    };

    public class TemperatureSensorEvent : PubSubEvent<TemperatureSensor>
    {
    };

}
