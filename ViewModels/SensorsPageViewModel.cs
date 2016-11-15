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
            Temperature = new TemperatureViewModel();

            // subscriptions
            _eventAggregator.GetEvent<SensorStatusEvent>().Subscribe(OnSensorStatus, ThreadOption.UIThread);
            _eventAggregator.GetEvent<TemperatureSensorEvent>().Subscribe(OnTemperatureSensor, ThreadOption.UIThread);

            // command implementation

            _logger.LogInfo("SensorPageViewModel.ctor", "success!");
        }

        #region Public Properties
        public TemperatureViewModel Temperature { get; private set; }

        public bool IsConnected
        {
            get { return _service.IsServiceInitialized; }
        }
        #endregion

        #region Commands
        #endregion

        #region Command Implemenation
        #endregion

        private void OnTemperatureSensor(TemperatureSensor args)
        {
            Temperature.Temperature = string.Format("{0:F2} C", args.Temperature);
            Temperature.Ambient = string.Format("{0:F2} C", args.Ambient);
        }

        private void OnSensorStatus(SensorStatus ss)
        {
            switch (ss.Sensor)
            {
                case Sensors.TEMPERATURE:
                    Temperature.Active = ss.Active;
                    break;
            }
        }
    }
}
