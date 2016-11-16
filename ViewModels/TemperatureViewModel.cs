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
    public class TemperatureViewModel : ViewModelBase
    {
        protected readonly IEventAggregator _eventAggregator;

        public TemperatureViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            // subscriptions
            _eventAggregator.GetEvent<PubSubEvent<SensorStatus>>().Subscribe(OnSensorStatus, ThreadOption.UIThread);
            _eventAggregator.GetEvent<PubSubEvent<TemperatureSensor>>().Subscribe(OnTemperatureSensor, ThreadOption.UIThread);

        }
        private bool _active;
        public bool Active
        {
            get { return _active; }
            set { SetProperty(ref _active, value); }
        }

        private string _temperature;
        public string Temperature
        {
            get { return _temperature; }
            set { SetProperty(ref _temperature, value); }
        }

        private string _ambient;
        public string Ambient
        {
            get { return _ambient; }
            set { SetProperty(ref _ambient, value); }
        }

        private void OnTemperatureSensor(TemperatureSensor args)
        {
            Active      = true;
            Temperature = string.Format("{0:F2} C", args.Temperature);
            Ambient     = string.Format("{0:F2} C", args.Ambient);
        }

        private void OnSensorStatus(SensorStatus ss)
        {
            switch (ss.Sensor)
            {
                case Sensors.TEMPERATURE:
                    Active = ss.Active;
                    break;
            }
        }
    }
}
