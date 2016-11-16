using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Windows.Mvvm;
using Prism.Events;

using SensorTagPi.Models;

namespace SensorTagPi.ViewModels
{
    public class OpticalViewModel : ViewModelBase
    {
        protected readonly IEventAggregator _eventAggregator;

        public OpticalViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            // subscriptions
            _eventAggregator.GetEvent<PubSubEvent<SensorStatus>>().Subscribe(OnSensorStatus, ThreadOption.UIThread);
            _eventAggregator.GetEvent<PubSubEvent<OpticalSensor>>().Subscribe(OnOpticalSensor, ThreadOption.UIThread);

        }
        private bool _active;
        public bool Active
        {
            get { return _active; }
            set { SetProperty(ref _active, value); }
        }

        private string _luminosity;
        public string Luminosity
        {
            get { return _luminosity; }
            set { SetProperty(ref _luminosity, value); }
        }

        private void OnOpticalSensor(OpticalSensor args)
        {
            Active = true;
            Luminosity = string.Format("{0:F2} lx", args.Luminosity);
        }

        private void OnSensorStatus(SensorStatus ss)
        {
            switch (ss.Sensor)
            {
                case Sensors.OPTICAL:
                    Active = ss.Active;
                    break;
            }
        }
    }
}
