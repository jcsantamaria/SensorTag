using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Events;

namespace SensorTagPi.Models
{
    /// <summary>
    /// Reports activity state from device sensors.
    /// </summary>
    public class SensorStatus
    {
        /// <summary>
        /// The sensor identifier.
        /// </summary>
        public Sensors Sensor { get; private set; }

        /// <summary>
        /// The activity state of the sensor.
        /// </summary>
        public bool    Active { get; private set; }

        /// <summary>
        /// Creates an sample instance.
        /// </summary>
        /// <param name="sensor">The sensor identifier.</param>
        /// <param name="active">The current state of the sensor.</param>
        public SensorStatus(Sensors sensor, bool active)
        {
            Sensor = sensor;
            Active = active;
        }
    };

    /// <summary>
    /// Reports a sample from the IR Temperature sensor.
    /// </summary>
    public class TemperatureSensor
    {
        /// <summary>
        /// Temperature at the object in Centrigrades (C).
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// Temperature of ambient in Centrigrades (C).
        /// </summary>
        public double Ambient { get; set; }

        /// <summary>
        /// Creates an sample instance.
        /// </summary>
        /// <param name="temperature">Current object temperature.</param>
        /// <param name="ambient">Current ambient temperature.</param>
        public TemperatureSensor( double temperature = double.NaN, double ambient = double.NaN)
        {
            Temperature  = temperature;
            Ambient      = ambient;
        }
    };

    /// <summary>
    /// Reports a sample from the Barometer sensor.
    /// </summary>
    public class BarometerSensor
    {
        /// <summary>
        /// Pressure in HectoPascals (hPa).
        /// </summary>
        public double Pressure { get; set; }

        /// <summary>
        /// Temperature in Centigrades (C).
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// Creates an sample instance.
        /// </summary>
        /// <param name="pressure">Current pressure in hPa.</param>
        /// <param name="temperature">Current temperature in C.</param>
        public BarometerSensor(double pressure = double.NaN, double temperature = double.NaN )
        {
            Pressure = pressure;
            Temperature = temperature;
        }
    };

    /// <summary>
    /// Reports button states from the Keys sensor.
    /// </summary>
    public class KeysSensor
    {
        /// <summary>
        /// State of the power button (small button).
        /// </summary>
        public bool PowerButton { get; set; }

        /// <summary>
        /// State of the options button (large button).
        /// </summary>
        public bool OptionButton { get; set; }

        /// <summary>
        /// Creates an sample instance.
        /// </summary>
        /// <param name="powerButton">Current state of the power button.</param>
        /// <param name="optionButton">Current state of the option button.</param>
        public KeysSensor(bool powerButton, bool optionButton)
        {
            PowerButton = powerButton;
            OptionButton = optionButton;
        }
    };

    /// <summary>
    /// Reports a sample from the Humidity sensor.
    /// </summary>
    public class HumiditySensor
    {
        /// <summary>
        /// Relative humidity in percentage (hPa).
        /// </summary>
        public double Humidity { get; set; }

        /// <summary>
        /// Temperature in Centigrades (C).
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// Creates an sample instance.
        /// </summary>
        /// <param name="humidity">Current relative humidity in percentage.</param>
        /// <param name="temperature">Current temperature in C.</param>
        public HumiditySensor(double humidity = double.NaN, double temperature = double.NaN)
        {
            Humidity = humidity;
            Temperature = temperature;
        }
    };

    /// <summary>
    /// Reports a sample from the Optical sensor.
    /// </summary>
    public class OpticalSensor
    {
        /// <summary>
        /// Light intensity in lux (lx).
        /// </summary>
        public double Luminosity { get; set; }

        /// <summary>
        /// Creates an sample instance.
        /// </summary>
        /// <param name="luminosity">Current light intensity in lux.</param>
        public OpticalSensor(double luminosity = double.NaN)
        {
            Luminosity = luminosity;
        }
    };
}
