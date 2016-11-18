using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    partial class IoTService : IIoTService
    {
        private readonly ILogger          _logger;
        private readonly IEventAggregator _eventAggregator;

        private DeviceClient _deviceClient;

        public IoTService(ILogger logger, IEventAggregator eventAggregator)
        {
            _logger          = logger;
            _eventAggregator = eventAggregator;

            _deviceClient = DeviceClient.Create(IotHubUri, AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey), TransportType.Http1);

            _eventAggregator.GetEvent<PubSubEvent<TemperatureSensor>>().Subscribe((data) => SendTemperatureToCloudMessagesAsync(data));
            _eventAggregator.GetEvent<PubSubEvent<OpticalSensor>>().Subscribe((data) => SendOpticalToCloudMessagesAsync(data));
            _eventAggregator.GetEvent<PubSubEvent<MovementSensor>>().Subscribe((data) => SendMovementToCloudMessagesAsync(data));

            _logger.LogInfo("IoTService.ctor", "Device: {0}", DeviceId);
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
    }
}
