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
    public class BarometerViewModel : ViewModelBase
    {
        protected readonly IEventAggregator _eventAggregator;

        public BarometerViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            // subscriptions
            _eventAggregator.GetEvent<SensorStatusEvent>().Subscribe(OnSensorStatus, ThreadOption.UIThread);
            _eventAggregator.GetEvent<BarometerSensorEvent>().Subscribe(OnBarometerSensor, ThreadOption.UIThread);

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

        private string _pressure;
        public string Pressure
        {
            get { return _pressure; }
            set { SetProperty(ref _pressure, value); }
        }

        private void OnBarometerSensor(BarometerSensor args)
        {
            Active      = true;
            Temperature = string.Format("{0:F2} C", args.Temperature);
            Pressure    = string.Format("{0:F2} hPa", args.Pressure);
        }

        private void OnSensorStatus(SensorStatus ss)
        {
            switch (ss.Sensor)
            {
                case Sensors.BAROMETER:
                    Active = ss.Active;
                    break;
            }
        }
    }
}
