﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Devices.Enumeration.Pnp;
using Windows.Storage.Streams;

using Prism.Events;

using SensorTagPi.Core.Interfaces;

namespace SensorTagPi.Models
{
    public enum Sensors
    {
        TEMPERATURE = 0,
        HUMIDITY = 1,
        BAROMETER = 2,
        MOVEMENT = 3,
        OPTICAL = 4,
        KEYS = 5,
    };

    public interface ISensorTagService
    {
        string SensorName { get; }

        bool IsServiceConnected { get; }

        Task ConnectServiceAsync(DeviceInformation device);

        void DisconnectService();

        Task EnableSensorAsync(Sensors sensor);
    }
   
    class SensorTagService : ISensorTagService
    {
        List<Guid> ServiceUuids = new List<Guid>(new [] {
                                new Guid("F000AA00-0451-4000-B000-000000000000"),
                                new Guid("F000AA20-0451-4000-B000-000000000000"),
                                new Guid("F000AA40-0451-4000-B000-000000000000"),
                                new Guid("F000AA80-0451-4000-B000-000000000000"),
                                new Guid("F000AA70-0451-4000-B000-000000000000"),
                                new Guid("0000FFE0-0000-1000-8000-00805F9B34FB"),
                              });

        List<Guid> NotificationUuids = new List<Guid>(new [] {
                                new Guid("F000AA01-0451-4000-B000-000000000000"),
                                new Guid("F000AA21-0451-4000-B000-000000000000"),
                                new Guid("F000AA41-0451-4000-B000-000000000000"),
                                new Guid("F000AA81-0451-4000-B000-000000000000"),
                                new Guid("F000AA71-0451-4000-B000-000000000000"),
                                new Guid("0000FFE1-0000-1000-8000-00805F9B34FB"),
                              });

        List<Guid> ConfigurationUuids = new List<Guid>(new[] {
                                new Guid("F000AA02-0451-4000-B000-000000000000"),
                                new Guid("F000AA22-0451-4000-B000-000000000000"),
                                new Guid("F000AA42-0451-4000-B000-000000000000"),
                                new Guid("F000AA82-0451-4000-B000-000000000000"),
                                new Guid("F000AA72-0451-4000-B000-000000000000"),
                                Guid.Empty,
                              });

        private readonly ILogger          _logger;
        private readonly IEventAggregator _eventAggregator;

        private GattDeviceService[]  _services;
        private GattCharacteristic[] _notifications;
        //private DeviceWatcher        _watcher;
        //private PnpObjectWatcher   pnpwatcher;

        public SensorTagService(ILogger logger, IEventAggregator eventAggregator)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;

            _services      = new GattDeviceService[Enum.GetValues(typeof(Sensors)).Length];
            _notifications = new GattCharacteristic[Enum.GetValues(typeof(Sensors)).Length];

            //_watcher = null;

            SensorName = string.Empty;
            IsServiceConnected = false;
        }

        #region ISensorTagService methods
        public string SensorName { get; set; }

        public bool IsServiceConnected { get; set; }

        public async Task ConnectServiceAsync(DeviceInformation device)
        {
            try
            {
                // form query for watcher
                StringBuilder aqs = new StringBuilder();
                aqs.AppendFormat("({0}) ", GattDeviceService.GetDeviceSelectorFromUuid(ServiceUuids[0]));
                foreach( var uuid in ServiceUuids.Skip(1))
                {
                    aqs.AppendFormat("OR ({0}) ", GattDeviceService.GetDeviceSelectorFromUuid(uuid));
                }

                var alldevs = await DeviceInformation.FindAllAsync(aqs.ToString(), new string[] { "System.Devices.ContainerId" });

                foreach ( var dev in alldevs)
                {
                    var svc = await GattDeviceService.FromIdAsync(dev.Id);
                    int index = ServiceUuids.IndexOf(svc.Uuid);
                    if (index >= 0)
                        _services[index] = svc;
                }

                int count = Enum.GetValues(typeof(Sensors)).Length;
                for (int i = 0; i<count; i++)
                {
                    if (_services[i] == null)
                    {
                        _logger.LogError("SensorTagService.ConnectServiceAsync", "Access to service {0} is denied, because the application was not granted access, " +
                                                                                    "or the device is currently in use by another application.", Enum.GetName(typeof(Sensors), i) );
                    }
                }

                // set properties
                IsServiceConnected = true;
                SensorName = device.Name;

                await ConfigureServiceForNotificationsAsync();

                _logger.LogInfo("SensorTagService.ConnectServiceAsync", "success!");
            }
            catch (Exception e)
            {
                _logger.LogException("SensorTagService.ConnectServiceAsync", e, "Accessing your device failed.");
            }
        }

        public void DisconnectService()
        {
            try
            {
                // disconnect all services
                for(int i =0; i<_services.Length; i++)
                {
                    var svc = _services[i];
                    if ( svc != null)
                        svc.Dispose();
                    _services[i] = null;
                }

                // release notifications
                for(int i=0; i< _notifications.Length; i++)
                    _notifications[i] = null;

                // reset properties
                SensorName = string.Empty;
                IsServiceConnected = false;

                _logger.LogInfo("SensorTagService.DisconnectService", "success!");
            }
            catch (Exception ex)
            {
                _logger.LogException("SensorTagService.DisconnectService", ex, string.Empty);
            }
        }
        #endregion

        #region Support methods
        /// <summary> 
        /// Configure the Bluetooth device to send notifications whenever the Characteristic value changes 
        /// </summary> 
        private async Task ConfigureServiceForNotificationsAsync()
        {
            try
            {
                int count = Enum.GetValues(typeof(Sensors)).Length;

                // enable notifications
                foreach (Sensors sensor in Enum.GetValues(typeof(Sensors)))
                {
                    int i = (int)sensor;
                    var svc = _services[i];
                    if (svc != null)
                    {
                        _logger.LogInfo("SensorTagService.ConfigureServiceForNotificationsAsync", "Configuring {0}", sensor);

                        var notification = svc.GetCharacteristics(NotificationUuids[i]).First();
                        if (notification.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                        {
                            _notifications[i] = notification;
                            switch (sensor)
                            {
                                case Sensors.BAROMETER:
                                    notification.ValueChanged += BarometerValueChanged;
                                    break;
                                case Sensors.HUMIDITY:
                                    notification.ValueChanged += HumidityValueChanged;
                                    break;
                                case Sensors.KEYS:
                                    notification.ValueChanged += KeysValueChanged;
                                    break;
                                case Sensors.MOVEMENT:
                                    notification.ValueChanged += MovementValueChanged;
                                    break;
                                case Sensors.OPTICAL:
                                    notification.ValueChanged += OpticalValueChanged;
                                    break;
                                case Sensors.TEMPERATURE:
                                    notification.ValueChanged += TemperatureValueChanged;
                                    break;
                                default:
                                    _logger.LogError("SensorTagService.ConfigureServiceForNotificationAsync", "Uknown sensor: {0}", sensor);
                                    break;
                            }

                            // Set the notify enable flag
                            await notification.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

                            _logger.LogInfo("SensorTagService.ConfigureServiceForNotificationsAsync", "Success! {0}", sensor);
                        }

                        // enable sensor
                        await EnableSensorAsync(sensor);
                    }
                }
            }
            catch( Exception ex)
            {
                _logger.LogException("SensorTagService.ConfigureServiceForNotificationsAsync", ex, string.Empty);
            }
        }

        public async Task EnableSensorAsync(Sensors sensor)
        {
            try
            {
                int i = (int)sensor;
                if (ConfigurationUuids[i] != Guid.Empty)
                {
                    var svc = _services[i];
                    if (svc != null)
                    {
                        var cnf = svc.GetCharacteristics(ConfigurationUuids[i]).First();
                        if (cnf.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write))
                        {
                            var writer = new DataWriter();
                            switch (sensor)
                            {
                                case Sensors.BAROMETER:
                                case Sensors.HUMIDITY:
                                case Sensors.OPTICAL:
                                case Sensors.TEMPERATURE:
                                    writer.WriteByte(0x01);
                                    break;

                                case Sensors.MOVEMENT:
                                    writer.WriteByte(0x7F);
                                    writer.WriteByte(0x00);
                                    break;
                            }

                            await cnf.WriteValueAsync(writer.DetachBuffer());
                        }
                    }

                    _eventAggregator.GetEvent<PubSubEvent<SensorStatus>>().Publish(new SensorStatus(sensor, true));

                    _logger.LogInfo("SensorTagService.EnableSensorAsync", "Sensor {0}: enabled!", sensor);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException("SensorTagService.EnableSensorAsync", ex, "Sensor {0}", sensor);
            }
        }

        private void TemperatureValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            try
            {
                byte[] buffer = new byte[args.CharacteristicValue.Length];
                DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(buffer);

                double objtemp = (BitConverter.ToInt16(buffer, 0) >> 2) * 0.03125;
                double ambtemp = (BitConverter.ToInt16(buffer, 2) >> 2) * 0.03125;

                _eventAggregator.GetEvent<PubSubEvent<TemperatureSensor>>().Publish(new TemperatureSensor(objtemp, ambtemp));

                //_logger.LogInfo("SensorTagService.TemperatureValueChanged", "Temperature: {0:F3}  Ambient: {1:F3}", objtemp, ambtemp);
            }
            catch (Exception ex)
            {
                _logger.LogException("SensorTagService.TemperatureValueChanged", ex, string.Empty);
            }
        }

        private void OpticalValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            try
            {
                byte[] buffer = new byte[args.CharacteristicValue.Length];
                DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(buffer);

                ushort data = BitConverter.ToUInt16(buffer, 0);
                int m = data & 0x0FFF;
                int e = (data & 0xF000) >> 12;
                double luminosity = m * 0.01 * Math.Pow(2, e);

                _eventAggregator.GetEvent<PubSubEvent<OpticalSensor>>().Publish(new OpticalSensor(luminosity));

                //_logger.LogInfo("SensorTagService.OpticalValueChanged", "Luminosity: {0:F3}", luminosity);

            }
            catch (Exception ex)
            {
                _logger.LogException("SensorTagService.OpticalValueChanged", ex, string.Empty);
            }
        }

        private void MovementValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            try
            {
                byte[] buffer = new byte[args.CharacteristicValue.Length];
                DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(buffer);

                // gyroscope data
                float gyrox = BitConverter.ToInt16(buffer, 0) * 500.0f / 65536.0f;
                float gyroy = BitConverter.ToInt16(buffer, 2) * 500.0f / 65536.0f;
                float gyroz = BitConverter.ToInt16(buffer, 4) * 500.0f / 65536.0f;

                // acceleration data: acceleration range configured to 2G
                float accx = BitConverter.ToInt16(buffer, 6) * 2.0f / 32768.0f;
                float accy = BitConverter.ToInt16(buffer, 8) * 2.0f / 32768.0f;
                float accz = BitConverter.ToInt16(buffer, 10) * 2.0f / 32768.0f;

                // magnetometer data
                float magx = BitConverter.ToInt16(buffer, 12);
                float magy = BitConverter.ToInt16(buffer, 14);
                float magz = BitConverter.ToInt16(buffer, 16);

                _eventAggregator.GetEvent<PubSubEvent<MovementSensor>>().Publish(new MovementSensor(new System.Numerics.Vector3(accx, accy, accz),
                                                                                                    new System.Numerics.Vector3(gyrox, gyroy, gyroz),
                                                                                                    new System.Numerics.Vector3(magx, magy, magz)));

                //_logger.LogInfo("SensorTagService.MovementValueChanged", "Acc: {0:F3},{1:F3},{2:F3}", accx, accy, accz);

            }
            catch (Exception ex)
            {
                _logger.LogException("SensorTagService.MovementValueChanged", ex, string.Empty);
            }
        }

        private void KeysValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            try
            {
                byte[] buffer = new byte[args.CharacteristicValue.Length];
                DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(buffer);

                byte data = buffer[0];

                _eventAggregator.GetEvent<PubSubEvent<KeysSensor>>().Publish(new KeysSensor((data & 0x02) != 0, (data & 0x01) != 0));

                _logger.LogInfo("SensorTagService.KeysValueChanged", "Keys: {0:X}", data);

            }
            catch (Exception ex)
            {
                _logger.LogException("SensorTagService.KeysValueChanged", ex, string.Empty);
            }
        }

        private void HumidityValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            try
            {
                byte[] buffer = new byte[args.CharacteristicValue.Length];
                DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(buffer);

                double temp = BitConverter.ToInt16(buffer, 0) * 165.0 / 65536.0 - 40.0;
                double humidity = BitConverter.ToInt16(buffer, 2) * 100.0 / 65536.0;

                _eventAggregator.GetEvent<PubSubEvent<HumiditySensor>>().Publish(new HumiditySensor(humidity, temp));

                //_logger.LogInfo("SensorTagService.HumidityValueChanged", "Temperature: {0:F3}  Humidity: {1:F3}", temp, humidity);

            }
            catch (Exception ex)
            {
                _logger.LogException("SensorTagService.HumidityValueChanged", ex, string.Empty);
            }
        }

        private void BarometerValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            try
            {
                byte[] buffer = new byte[args.CharacteristicValue.Length];
                DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(buffer);

                double temp = BitConverter.ToInt32(new byte[] { buffer[0], buffer[1], buffer[2], 0x00 }, 0) / 100.0;
                double pres = BitConverter.ToInt32(new byte[] { buffer[3], buffer[4], buffer[5], 0x00 }, 0) / 100.0;

                _eventAggregator.GetEvent<PubSubEvent<BarometerSensor>>().Publish(new BarometerSensor(pres, temp));

                //_logger.LogInfo("SensorTagService.BarometerValueChanged", "Temperature: {0:F3}  Pressure: {1:F3}", temp, pres);

            }
            catch (Exception ex)
            {
                _logger.LogException("SensorTagService.TemperatureValueChanged", ex, string.Empty);
            }
        }
        #endregion
    }
}
