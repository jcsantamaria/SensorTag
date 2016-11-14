using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Windows.Mvvm;

using SensorTagPi.Models;

namespace SensorTagPi.ViewModels
{
    public class SensorViewModel : ViewModelBase
    {
        private readonly Sensors _sensor;

        public SensorViewModel(Sensors sensor)
        {
            _sensor = sensor;
        }

        public Sensors Sensor
        {
            get { return _sensor; }
        }

        public string Name
        {
            get { return Enum.GetName(typeof(Sensors), _sensor); }
        }

        private bool _status;
        public bool Status
        {
            get { return _status; }

            set { SetProperty(ref _status, value); }
        }
    }

}
