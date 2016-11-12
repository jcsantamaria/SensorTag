using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Threading;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;

using Microsoft.Practices.ServiceLocation;
using Prism.Windows.Mvvm;
using Prism.Commands;

using SensorTagPi.Core.Interfaces;

namespace SensorTagPi.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        const double HorizontalResolution = 2.0;
        const double VerticalResolution   = 2.0;

        private readonly ILogger         _logger;
        private readonly TaskScheduler   _uiContext;
        private WiFiAdapter              _wifi;
        private bool                     _isBusy;

        public MainPageViewModel(ILogger logger)
        {
            //_logger    = ServiceLocator.Current.GetInstance<ILogger>();
            _logger    = logger;
            _uiContext = TaskScheduler.FromCurrentSynchronizationContext();
            _isBusy    = false;

            // command implementation
            ConnectCommand    = new DelegateCommand(DoConnectCommand, CanDoConnectCommand);
            DisconnectCommand = new DelegateCommand(DoDisconnectCommand, CanDoDisconnectCommand);

            // notify all change
            OnPropertyChanged(() => IsConnected);
        }

        #region Public Properties
        private string _networkID = string.Empty;
        public string NetworkID
        {
            get { return _networkID; }

            set { SetProperty(ref _networkID, value); }
        }

        private ushort _sensorPort = 2368;
        public ushort SensorPort
        {
            get { return _sensorPort; }

            set
            {
                if ( !IsConnected )
                    SetProperty(ref _sensorPort, value);
            }
        }

        private ushort _remotePort = 8787;
        public ushort RemotePort
        {
            get { return _remotePort; }

            set
            {
                if (!IsConnected)
                    SetProperty(ref _remotePort, value);
            }
        }

        private string _remoteIP = "192.168.0.100";
        public string RemoteIP
        {
            get { return _remoteIP; }

            set
            {
                SetProperty(ref _remoteIP, value);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if station is connected.
        /// </summary>
        public bool IsConnected
        {
            get { return _sensor.IsConnected; }
        }

        private int _interval = 200;
        public int Interval
        {
            get { return _interval; }

            set { SetProperty(ref _interval, value); }
        }

        private ulong _packetCount = 0;
        public ulong RxMessages
        {
            get { return _packetCount; }

            set { SetProperty(ref _packetCount, value); }
        }
        #endregion

        #region Commands
        public DelegateCommand ConnectCommand { get; private set; }
        public DelegateCommand DisconnectCommand { get; private set; }
        #endregion

        #region Command Implemenation
        internal async void DoConnectCommand()
        {
            try
            {
                // get the wifi adapter (if needed)
                if (_wifi == null)
                {
                    _wifi = (await WiFiAdapter.FindAllAdaptersAsync()).First();
                    _wifi.AvailableNetworksChanged += OnAvailableNetworksChanged;

                    // scan for networks
                    await _wifi.ScanAsync();
                }

                await _sensor.Connect(_sensorPort, _remoteIP, _remotePort);

                // update ui state
                OnPropertyChanged(() => IsConnected);
                ConnectCommand.RaiseCanExecuteChanged();
                DisconnectCommand.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                _logger.LogException("MainPageViewModel.DoConnectCommand", ex, string.Empty);
            }
        }

        private async void OnAvailableNetworksChanged(WiFiAdapter sender, object args)
        {
            if (_isBusy)
                return;

            // restablish wifi connection (if applicable)
            var profile = NetworkInformation.GetInternetConnectionProfile();
            if (profile == null || !profile.ProfileName.StartsWith("RPT_"))
            {
                var network = _wifi.NetworkReport.AvailableNetworks.Where(nw => nw.Ssid.StartsWith("RPT_")).FirstOrDefault();
                if (network != null)
                {
                    _isBusy = true;
                    // connect to this network
                    var result = await _wifi.ConnectAsync(network, WiFiReconnectionKind.Automatic);
                    // notify ui
                    await Task.Factory.StartNew(() => NetworkID = network.Ssid, CancellationToken.None, TaskCreationOptions.None, _uiContext);
                    _isBusy = false;
                    _logger.LogInfo("MainPageViewModel.OnAvailableNetworksChanged", "network: {0}: {1}", _networkID, result.ConnectionStatus);
                }
                else
                {
                    _isBusy = true;
                    // notify ui
                    await Task.Factory.StartNew(() => NetworkID = string.Empty, CancellationToken.None, TaskCreationOptions.None, _uiContext);
                    _isBusy = false;
                    _logger.LogInfo("MainPageViewModel.OnAvailableNetworksChanged", "no sensor network available");
                }
            }
        }

        internal bool CanDoConnectCommand()
        {
            return !_sensor.IsConnected;
        }

        internal async void DoDisconnectCommand()
        {
            try
            {
                await _sensor.Disconnect();

                // update ui state
                OnPropertyChanged(() => IsConnected);
                ConnectCommand.RaiseCanExecuteChanged();
                DisconnectCommand.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                _logger.LogException("MainPageViewModel.DoDisconnectCommand", ex, string.Empty);
            }
        }

        internal bool CanDoDisconnectCommand()
        {
            return _sensor.IsConnected;
        }

        #endregion

        #region Support methods

        private void OnTimerTick(object sender, object e)
        {
            RemoteIP   = _sensor.RemoteHost;
            RxMessages = _sensor.RxMessages;
        }
        #endregion
    }
}
