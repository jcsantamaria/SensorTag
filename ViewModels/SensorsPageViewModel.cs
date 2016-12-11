using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using Prism.Events;
using Prism.Commands;

using SensorTagPi.Core.Interfaces;
using SensorTagPi.Models;

namespace SensorTagPi.ViewModels
{
    class SensorsPageViewModel : PageViewModelBase
    {
        public const string Token = "Sensors";

        private readonly ISensorTagService  _service;

        public SensorsPageViewModel(ILogger logger, IEventAggregator eventAggregator, INavigationService navigation, ISensorTagService service) : base(logger, eventAggregator, navigation)
        {
            _service = service;

            // sensor view models
            Temperature = new TemperatureViewModel(_eventAggregator);
            Barometer   = new BarometerViewModel(_eventAggregator);
            Humidity    = new HumidityViewModel(_eventAggregator);
            Optical     = new OpticalViewModel(_eventAggregator);
            Movement    = new MovementViewModel(_eventAggregator);
            Keys        = new KeysViewModel(_eventAggregator);

            // subscribe to measurements
            _eventAggregator.GetEvent<PubSubEvent<Measurements>>().Subscribe((data) => Messages = Messages + 1, ThreadOption.UIThread);

            // command implementation
            DisconnectCommand = new DelegateCommand(DoDisconnectCommand, CanDoDisconnectCommand);

            _logger.LogInfo("SensorPageViewModel.ctor", "success!");
        }

        #region Public Properties
        public string SensorName
        {
            get { return _service.SensorName; }
        }

        public TemperatureViewModel Temperature { get; private set; }
        public BarometerViewModel   Barometer { get; private set; }
        public HumidityViewModel    Humidity { get; private set; }
        public OpticalViewModel     Optical { get; private set; }
        public MovementViewModel    Movement { get; private set; }
        public KeysViewModel        Keys { get; private set; }

        private uint _messages;
        public uint Messages
        {
            get { return _messages; }
            set { SetProperty(ref _messages, value); }
        }

        public bool IsConnected
        {
            get { return _service.IsServiceConnected; }
        }
        #endregion

        #region Commands
        public DelegateCommand DisconnectCommand { get; private set; }
        #endregion

        #region Command Implemenation
        void DoDisconnectCommand()
        {
            if (_service.IsServiceConnected)
            {
                _service.DisconnectService();

                OnPropertyChanged(() => SensorName);
                OnPropertyChanged(() => IsConnected);

                _navigation.Navigate(DevicesPageViewModel.Token, null);
            }
        }

        bool CanDoDisconnectCommand()
        {
            return _service.IsServiceConnected;
        }
        #endregion
    }
}
