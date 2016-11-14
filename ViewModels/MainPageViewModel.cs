using System;
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

using SensorTagPi.Core.Interfaces;
using SensorTagPi.Models;
using Prism.Events;

namespace SensorTagPi.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly ILogger           _logger;
        private readonly IEventAggregator  _eventAggregator;
        private readonly ISensorTagService _service;
        private readonly TaskScheduler     _uiContext;
        private DeviceWatcher _watcher;
        private WiFiAdapter   _wifi;
        private bool          _isBusy;
        private Dictionary<Sensors, SensorViewModel> _sensors;

        public MainPageViewModel(ILogger logger, IEventAggregator eventAggregator, ISensorTagService service)
        {
            //_logger    = ServiceLocator.Current.GetInstance<ILogger>();
            _logger          = logger;
            _eventAggregator = eventAggregator;
            _service         = service;
            _uiContext       = TaskScheduler.FromCurrentSynchronizationContext();

            _watcher = null;
            _wifi   = null;
            _isBusy = false;
            _sensors = Enum.GetValues(typeof(Sensors)).Cast<Sensors>().Select(s => new SensorViewModel(s)).ToDictionary(svm => svm.Sensor);

            // command implementation
            ScanCommand    = new DelegateCommand(DoScanCommand, CanDoScanCommand);
            ConnectCommand = new DelegateCommand(DoConnectCommand, CanDoConnectCommand);

            _eventAggregator.GetEvent<SensorStatusEvent>().Subscribe(ss => _sensors[ss.Sensor].Status = ss.Active);

            _logger.LogInfo("MainPageViewMode.ctor", "success!");
        }


        #region Public Properties
        private string _networkID = string.Empty;
        public string NetworkID
        {
            get { return _networkID; }

            set { SetProperty(ref _networkID, value); }
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

        public IList<SensorViewModel> Sensors
        {
            get { return _sensors.Values.ToList(); }
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
            if ( _selectedIndex >= 0 && !_service.IsServiceInitialized)
            {
                var deviceInfo = _devices[_selectedIndex];
                await _service.InitializeServiceAsync(deviceInfo);
            }
        }

        bool CanDoConnectCommand()
        {
            return _selectedIndex >= 0 && !_service.IsServiceInitialized;
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
