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
    public class HumidityViewModel : ViewModelBase
    {
        protected readonly IEventAggregator _eventAggregator;

        public HumidityViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            // subscriptions
            _eventAggregator.GetEvent<PubSubEvent<SensorStatus>>().Subscribe(OnSensorStatus, ThreadOption.UIThread);
            _eventAggregator.GetEvent<PubSubEvent<HumiditySensor>>().Subscribe(OnHumiditySensor, ThreadOption.UIThread);

        }
        private bool _active;
        public bool Active
        {
            get { return _active; }
            set { SetProperty(ref _active, value); }
        }

        private string _humidity;
        public string Humidity
        {
            get { return _humidity; }
            set { SetProperty(ref _humidity, value); }
        }

        private string _temperature;
        public string Temperature
        {
            get { return _temperature; }
            set { SetProperty(ref _temperature, value); }
        }

        private void OnHumiditySensor(HumiditySensor args)
        {
            Active = true;
            Humidity = string.Format("{0:F2} %", args.Humidity);
            Temperature = string.Format("{0:F2} C", args.Temperature);
        }

        private void OnSensorStatus(SensorStatus ss)
        {
            switch (ss.Sensor)
            {
                case Sensors.HUMIDITY:
                    Active = ss.Active;
                    break;
            }
        }
    }
}
