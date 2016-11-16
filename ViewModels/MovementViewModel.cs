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
    public class MovementViewModel : ViewModelBase
    {
        protected readonly IEventAggregator _eventAggregator;

        public MovementViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            // subscriptions
            _eventAggregator.GetEvent<PubSubEvent<SensorStatus>>().Subscribe(OnSensorStatus, ThreadOption.UIThread);
            _eventAggregator.GetEvent<PubSubEvent<MovementSensor>>().Subscribe(OnMovementSensor, ThreadOption.UIThread);

        }
        private bool _active;
        public bool Active
        {
            get { return _active; }
            set { SetProperty(ref _active, value); }
        }

        private string _accelerometer;
        public string Accelerometer
        {
            get { return _accelerometer; }
            set { SetProperty(ref _accelerometer, value); }
        }

        private string _gyroscope;
        public string Gyroscope
        {
            get { return _gyroscope; }
            set { SetProperty(ref _gyroscope, value); }
        }

        private string _magnetometer;
        public string Magnetometer
        {
            get { return _magnetometer; }
            set { SetProperty(ref _magnetometer, value); }
        }

        private void OnMovementSensor(MovementSensor args)
        {
            Active = true;
            Accelerometer = string.Format("{0,6:F2},{1,6:F2},{2,6:F2} G", args.Accelerometer.X, args.Accelerometer.Y, args.Accelerometer.Z);
            Gyroscope     = string.Format("{0,6:F2},{1,6:F2},{2,6:F2} deg/s", args.Gyroscope.X, args.Gyroscope.Y, args.Gyroscope.Z);
            Magnetometer  = string.Format("{0,6:F2},{1,6:F2},{2,6:F2} μT", args.Magnetometer.X, args.Magnetometer.Y, args.Magnetometer.Z);
        }

        private void OnSensorStatus(SensorStatus ss)
        {
            switch (ss.Sensor)
            {
                case Sensors.MOVEMENT:
                    Active = ss.Active;
                    break;
            }
        }
    }
}
