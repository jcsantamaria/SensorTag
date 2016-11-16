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
            Temperature = new TemperatureViewModel(eventAggregator);
            Barometer   = new BarometerViewModel(eventAggregator);
            Keys        = new KeysViewModel(eventAggregator);

            // command implementation

            _logger.LogInfo("SensorPageViewModel.ctor", "success!");
        }

        #region Public Properties
        public string SensorName
        {
            get { return _service.SensorName; }
        }

        public TemperatureViewModel Temperature { get; private set; }
        public BarometerViewModel   Barometer { get; private set; }
        public KeysViewModel        Keys { get; private set; }

        public bool IsConnected
        {
            get { return _service.IsServiceInitialized; }
        }
        #endregion

        #region Commands
        #endregion

        #region Command Implemenation
        #endregion

    }
}
