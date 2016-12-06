using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Azure.Devices.Client;

using Prism.Events;

using Newtonsoft.Json;

using SensorTagPi.Core.Interfaces;

namespace SensorTagPi.Models
{
    interface IIoTService
    {
        void SendTemperatureToCloudMessagesAsync(TemperatureSensor movement);
        void SendOpticalToCloudMessagesAsync(OpticalSensor movement);
        void SendMovementToCloudMessagesAsync(MovementSensor movement);
        void SendHumidityToCloudMessagesAsync(HumiditySensor humidity);
    }

    class Triplet
    {
        public DateTime Time { get; set; }
        public string   Measurement { get; set; }
        public string   Unit { get; set; }
        public float    Value { get; set; }

        public Triplet(string measurement, float value, string unit)
        {
            Time        = DateTime.UtcNow;
            Measurement = measurement;
            Value       = value;
            Unit        = unit;
        }

        public static List<Triplet> FromOptical(OpticalSensor optical)
        {
            var result = new List<Triplet>();
            result.Add(new Triplet("Luminosity", (float)optical.Luminosity, "lux"));
            return result;
        }

        public static List<Triplet> FromTemperature(TemperatureSensor temperature)
        {
            var result = new List<Triplet>();
            result.Add(new Triplet("IRTemperature", (float)temperature.Temperature, "C"));
            result.Add(new Triplet("IRAmbient", (float)temperature.Ambient, "C"));
            return result;
        }

        public static List<Triplet> FromHumidity(HumiditySensor humidity)
        {
            var result = new List<Triplet>();
            result.Add(new Triplet("Humidity", (float)humidity.Humidity, "%"));
            result.Add(new Triplet("HumidityTemperature", (float)humidity.Temperature, "C"));
            return result;
        }

        public static List<Triplet> FromBarometer(BarometerSensor barometer)
        {
            var result = new List<Triplet>();
            result.Add(new Triplet("Pressure", (float)barometer.Pressure, "hPa"));
            result.Add(new Triplet("BarometerTemperature", (float)barometer.Temperature, "C"));
            return result;
        }

        public static List<Triplet> FromMovement(MovementSensor movement)
        {
            var result = new List<Triplet>();
            result.Add(new Triplet("AccX", movement.Accelerometer.X, "G"));
            result.Add(new Triplet("AccY", movement.Accelerometer.Y, "G"));
            result.Add(new Triplet("AccZ", movement.Accelerometer.Z, "G"));
            result.Add(new Triplet("GyrX", movement.Gyroscope.X, "deg/s"));
            result.Add(new Triplet("GyrY", movement.Gyroscope.Y, "deg/s"));
            result.Add(new Triplet("GyrZ", movement.Gyroscope.Z, "deg/s"));
            result.Add(new Triplet("MagX", movement.Magnetometer.X, "uT"));
            result.Add(new Triplet("MagY", movement.Magnetometer.Y, "uT"));
            result.Add(new Triplet("MagZ", movement.Magnetometer.Z, "uT"));
            return result;
        }
    }

    class Measurements
    {
        public DateTime Time { get; set; }

        public float Temperature { get; set; }

        public float Humidity { get; set; }

        public float Pressure { get; set; }

        public float Luminosity { get; set; }

        public Measurements(float temperature, float humidity, float pressure, float luminosity)
        {
            Time        = DateTime.UtcNow;
            Temperature = temperature;
            Humidity    = humidity;
            Pressure    = pressure;
            Luminosity  = luminosity;
        }
    }

    partial class IoTService : IIoTService
    {
        private readonly ILogger          _logger;
        private readonly IEventAggregator _eventAggregator;

        private DeviceClient   _deviceClient;
        private Timer          _timer;
        private List<Triplet>  _measurements;
        private int            _sending;

        public IoTService(ILogger logger, IEventAggregator eventAggregator, int period)
        {
            _logger          = logger;
            _eventAggregator = eventAggregator;

            _deviceClient = DeviceClient.Create(IotHubUri, AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey), TransportType.Http1);

            _eventAggregator.GetEvent<PubSubEvent<TemperatureSensor>>().Subscribe((data) => CollectTemperatureMeasurements(data));
            _eventAggregator.GetEvent<PubSubEvent<OpticalSensor>>().Subscribe((data) => CollectOpticalMeasurements(data));
            _eventAggregator.GetEvent<PubSubEvent<HumiditySensor>>().Subscribe((data) => CollectHumidityMeasurements(data));
            _eventAggregator.GetEvent<PubSubEvent<BarometerSensor>>().Subscribe((data) => CollectBarometerMeasurements(data));
            //_eventAggregator.GetEvent<PubSubEvent<MovementSensor>>().Subscribe((data) => SendMovementToCloudMessagesAsync(data));

            // create measurement list
            _measurements = new List<Triplet>();
            _sending = 0;

            // create timer
            _timer = new Timer(OnTimer, _deviceClient, period, period);

            _logger.LogInfo("IoTService.ctor", "Device: {0}", DeviceId);
        }

        private void OnTimer(object state)
        {
            // aggregate all measurements (only if we are not busy already)
            if (Interlocked.CompareExchange(ref _sending, 1, 0) == 0)
            {
                try
                {
                    Measurements aggregate = null;
                    lock (_measurements)
                    {
                        if (_measurements.Count > 0)
                        {
                            float temperature = 0f;
                            var vals = _measurements.Where(trp => trp.Measurement == "IRAmbient").Select(trp => trp.Value);
                            if (vals.Any())
                                temperature = vals.Average();

                            float humidity = 0f;
                            vals = _measurements.Where(trp => trp.Measurement == "Humidity").Select(trp => trp.Value);
                            if (vals.Any())
                                humidity = vals.Average();

                            float pressure = 0f;
                            vals = _measurements.Where(trp => trp.Measurement == "Pressure").Select(trp => trp.Value);
                            if (vals.Any())
                                pressure = vals.Average();

                            float luminosity = 0f;
                            vals = _measurements.Where(trp => trp.Measurement == "Luminosity").Select(trp => trp.Value);
                            if (vals.Any())
                                luminosity = vals.Average();

                            aggregate = new Measurements(temperature, humidity, pressure, luminosity);
                        }
                    }

                    // send to the cloud
                    if (aggregate != null)
                    {
                        string serial = JsonConvert.SerializeObject(aggregate);
                        _deviceClient.SendEventAsync(new Message(Encoding.ASCII.GetBytes(serial))).AsTask().Wait();
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogException("IoTService.OnTimer", ex, string.Empty);
                }
            }

            // done
            Interlocked.Exchange(ref _sending, 0);
        }

        public async void SendTemperatureToCloudMessagesAsync(TemperatureSensor temperature)
        {
            try
            {
                var messages = new List<Message>();
                foreach (var triplet in Triplet.FromTemperature(temperature))
                {
                    string serial = JsonConvert.SerializeObject(triplet);
                    messages.Add(new Message(Encoding.ASCII.GetBytes(serial)));
                }

                if (messages.Count > 0)
                    await _deviceClient.SendEventBatchAsync(messages);
            }
            catch (Exception ex)
            {
                _logger.LogException("IoTService.SendTemperatureToCloudMessagesAsync", ex, string.Empty);
            }
        }

        public async void SendOpticalToCloudMessagesAsync(OpticalSensor optical)
        {
            try
            {
                var messages = new List<Message>();
                foreach (var triplet in Triplet.FromOptical(optical))
                {
                    string serial = JsonConvert.SerializeObject(triplet);
                    messages.Add(new Message(Encoding.ASCII.GetBytes(serial)));
                }

                if (messages.Count > 0)
                    await _deviceClient.SendEventBatchAsync(messages);
            }
            catch (Exception ex)
            {
                _logger.LogException("IoTService.SendOpticalToCloudMessagesAsync", ex, string.Empty);
            }
        }

        public async void SendHumidityToCloudMessagesAsync(HumiditySensor humidity)
        {
            try
            {
                var messages = new List<Message>();
                foreach (var triplet in Triplet.FromHumidity(humidity))
                {
                    string serial = JsonConvert.SerializeObject(triplet);
                    messages.Add(new Message(Encoding.ASCII.GetBytes(serial)));
                }

                if (messages.Count > 0)
                    await _deviceClient.SendEventBatchAsync(messages);
            }
            catch (Exception ex)
            {
                _logger.LogException("IoTService.SendHumidityToCloudMessagesAsync", ex, string.Empty);
            }
        }

        public async void SendMovementToCloudMessagesAsync(MovementSensor movement)
        {
            try
            {
                var messages = new List<Message>();
                foreach(var triplet in Triplet.FromMovement(movement))
                {
                    string serial = JsonConvert.SerializeObject(triplet);
                    messages.Add(new Message(Encoding.ASCII.GetBytes(serial)));
                }

                if ( messages.Count > 0 )
                    await _deviceClient.SendEventBatchAsync(messages);
            }
            catch (Exception ex)
            {
                _logger.LogException("IoTService.SendMovementToCloudMessagesAsync", ex, string.Empty);
            }
        }

        private void CollectTemperatureMeasurements(TemperatureSensor temperature)
        {
            lock(_measurements)
            {
                _measurements.AddRange(Triplet.FromTemperature(temperature));
            }
        }

        private void CollectOpticalMeasurements(OpticalSensor optical)
        {
            lock (_measurements)
            {
                _measurements.AddRange(Triplet.FromOptical(optical));
            }
        }

        private void CollectHumidityMeasurements(HumiditySensor humidity)
        {
            lock (_measurements)
            {
                _measurements.AddRange(Triplet.FromHumidity(humidity));
            }
        }
        private void CollectBarometerMeasurements(BarometerSensor barometer)
        {
            lock (_measurements)
            {
                _measurements.AddRange(Triplet.FromBarometer(barometer));
            }
        }
    }
}
