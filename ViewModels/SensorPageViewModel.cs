using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Windows.Mvvm;
using Prism.Events;

using SensorTagPi.Core.Interfaces;
using SensorTagPi.Models;

namespace SensorTagPi.ViewModels
{
    class SensorPageViewModel : ViewModelBase
    {
        private readonly ILogger           _logger;
        private readonly IEventAggregator  _eventAggregator;
        private readonly ISensorTagService _service;
        private readonly TaskScheduler     _uiContext;

        public SensorPageViewModel(ILogger logger, IEventAggregator eventAggregator, ISensorTagService service)
        {
            _logger          = logger;
            _eventAggregator = eventAggregator;
            _service         = service;
            _uiContext       = TaskScheduler.FromCurrentSynchronizationContext();

            // sensor view models
            Temperature = new TemperatureViewModel();

            _eventAggregator.GetEvent<SensorStatusEvent>().Subscribe(OnSensorStatus, ThreadOption.UIThread);
            _eventAggregator.GetEvent<TemperatureSensorEvent>().Subscribe(OnTemperatureSensor, ThreadOption.UIThread);

            _logger.LogInfo("SensorPageViewModel.ctor", "success!");
        }

        #region Public Properties
        public TemperatureViewModel Temperature { get; private set; }

        public bool IsConnected
        {
            get { return _service.IsServiceInitialized; }
        }
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
