using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Devices.Enumeration.Pnp;

using SensorTagPi.Core.Interfaces;

namespace SensorTagPi.Models
{
    public interface ISensorTagService
    {
        bool IsServiceInitialized { get; }

        Task InitializeServiceAsync(DeviceInformation device);
    }

    class SensorTagService : ISensorTagService
    {
        private readonly ILogger _logger;

        private GattDeviceService  service;
        private GattCharacteristic characteristic;
        private PnpObjectWatcher   watcher;
        private string             deviceContainerId;

        public SensorTagService(ILogger logger)
        {
            _logger = logger;

            service        = null;
            characteristic = null;

            IsServiceInitialized = false;
        }

        #region ISensorTagService methods
        public bool IsServiceInitialized { get; set; }

        public async Task InitializeServiceAsync(DeviceInformation device)
        {
            try
            {
                deviceContainerId = "{" + device.Properties["System.Devices.ContainerId"] + "}";

                service = await GattDeviceService.FromIdAsync(device.Id);
                if (service != null)
                {
                    IsServiceInitialized = true;
                    await ConfigureServiceForNotificationsAsync();
                }
                else
                {
                    _logger.LogError("SensorTagService.InitializeServiceAsync", "Access to the device is denied, because the application was not granted access, " +
                                                                                "or the device is currently in use by another application.");
                }
            }
            catch (Exception e)
            {
                _logger.LogException("SensorTagService.InitializeServiceAsync", e, "Accessing your device failed.");
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
                GattCharacteristicUuids.
                // Obtain the characteristic for which notifications are to be received 
                characteristic = service.GetCharacteristics(CHARACTERISTIC_UUID)[CHARACTERISTIC_INDEX];

                // While encryption is not required by all devices, if encryption is supported by the device, 
                // it can be enabled by setting the ProtectionLevel property of the Characteristic object. 
                // All subsequent operations on the characteristic will work over an encrypted link. 
                characteristic.ProtectionLevel = GattProtectionLevel.EncryptionRequired;

                // Register the event handler for receiving notifications 
                characteristic.ValueChanged += Characteristic_ValueChanged;

                // In order to avoid unnecessary communication with the device, determine if the device is already  
                // correctly configured to send notifications. 
                // By default ReadClientCharacteristicConfigurationDescriptorAsync will attempt to get the current 
                // value from the system cache and communication with the device is not typically required. 
                var currentDescriptorValue = await characteristic.ReadClientCharacteristicConfigurationDescriptorAsync();

                if ((currentDescriptorValue.Status != GattCommunicationStatus.Success) ||
                (currentDescriptorValue.ClientCharacteristicConfigurationDescriptor != CHARACTERISTIC_NOTIFICATION_TYPE))
                {
                    // Set the Client Characteristic Configuration Descriptor to enable the device to send notifications 
                    // when the Characteristic value changes 
                    GattCommunicationStatus status =
await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
CHARACTERISTIC_NOTIFICATION_TYPE);

                    if (status == GattCommunicationStatus.Unreachable)
                    {
                        // Register a PnpObjectWatcher to detect when a connection to the device is established, 
                        // such that the application can retry device configuration. 
                        StartDeviceConnectionWatcher();
                    }
                }
            }
            catch (Exception e)
            {
                rootPage.NotifyUser("ERROR: Accessing your device failed." + Environment.NewLine + e.Message,
                NotifyType.ErrorMessage);
            }
        }
        #endregion
    }
}
