﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Threading;
using System.Numerics;
using System.Collections.ObjectModel;

using Windows.UI.Xaml;
using Windows.Devices.WiFi;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;
using Windows.Networking.Connectivity;

using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Events;
using Prism.Windows.Navigation;

using SensorTagPi.Core.Interfaces;
using SensorTagPi.Models;

namespace SensorTagPi.ViewModels
{
    public class DevicesPageViewModel : PageViewModelBase
    {
        public const string Token = "Devices";

        private readonly ISensorTagService  _service;
        private DeviceWatcher _watcher;
        private WiFiAdapter   _wifi;
        private bool          _isBusy;

        public DevicesPageViewModel(ILogger logger, IEventAggregator eventAggregator, INavigationService navigation, ISensorTagService service) : base(logger, eventAggregator, navigation)
        {
            _service = service;

            _watcher = null;
            _wifi   = null;
            _isBusy = false;

            // command implementation
            ScanCommand     = new DelegateCommand(DoScanCommand, CanDoScanCommand);
            ConnectCommand  = new DelegateCommand(DoConnectCommand, CanDoConnectCommand);

            _logger.LogInfo("MainPageViewMode.ctor", "success!");
        }

        #region Public Properties
        private string _networkID = string.Empty;
        public string NetworkID
        {
            get { return _networkID; }

            set { SetProperty(ref _networkID, value); }
        }

        public string SensorName
        {
            get { return _service.SensorName; }
        }

        public bool IsConnected
        {
            get { return _service.IsServiceConnected; }
        }

        private ObservableCollection<DeviceInformation> _devices = new ObservableCollection<DeviceInformation>();
        public ObservableCollection<DeviceInformation> Devices
        {
            get { return _devices; }
        }

        private int _selectedIndex = -1;
        public int SelectedDeviceIndex
        {
            get { return _selectedIndex; }

            set
            {
                SetProperty(ref _selectedIndex, value);
                ConnectCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Commands
        public DelegateCommand ScanCommand { get; private set; }
        public DelegateCommand ConnectCommand { get; private set; }
        #endregion

        #region Command Implemenation
        void DoScanCommand()
        {
            if ( _watcher == null )
            {
                //string aqs = BluetoothLEDevice.GetDeviceSelectorFromDeviceName("SensorTag*");
                string aqs = BluetoothLEDevice.GetDeviceSelector();
                _watcher = DeviceInformation.CreateWatcher(aqs);

                _watcher.EnumerationCompleted += OnWatcherEnumerationCompleted;
                _watcher.Added += OnWatcherAdded;
                _watcher.Removed += OnWatcherRemoved;

                _watcher.Start();

                _logger.LogInfo("MainPageViewModel.DoScanCommand", "watcher: {0}", aqs);
            }
        }

        bool CanDoScanCommand()
        {
            return _watcher == null;
        }

        async void DoConnectCommand()
        {
            if ( _selectedIndex >= 0 && !_service.IsServiceConnected)
            {
                var deviceInfo = _devices[_selectedIndex];
                await _service.ConnectServiceAsync(deviceInfo);

                OnPropertyChanged(() => SensorName);
                OnPropertyChanged(() => IsConnected);

                _navigation.Navigate(SensorsPageViewModel.Token, null);
            }
        }

        bool CanDoConnectCommand()
        {
            return _selectedIndex >= 0 && !_service.IsServiceConnected;
        }
        #endregion

        #region Support methods
        void OnWatcherRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            _logger.LogInfo("MainPageViewModel.OnWatcherRemoved", "Id: {0}", args.Id);
            Task.Factory.StartNew(() =>
                    {
                        var deviceInfo = _devices.FirstOrDefault(di => di.Id == args.Id);
                        if (deviceInfo != null)
                            _devices.Remove(deviceInfo);
                    },
                CancellationToken.None, TaskCreationOptions.None, _uiContext);
        }

        void OnWatcherAdded(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            _logger.LogInfo("MainPageViewModel.OnWatcherAdded", "Found: {0}", deviceInfo.Name);
            Task.Factory.StartNew(() =>
                    {
                        _devices.Add(deviceInfo);
                    },
                CancellationToken.None, TaskCreationOptions.None, _uiContext);
        }

        void OnWatcherEnumerationCompleted(DeviceWatcher sender, object args)
        {
            _logger.LogInfo("MainPageViewModel.OnWatcherEnumerationCompleted", "Enumeration completed");
            _watcher.Stop();
            _watcher = null;
        }

        #endregion
    }
}
