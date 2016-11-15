using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Windows.Mvvm;

namespace SensorTagPi.ViewModels
{
    public class TemperatureViewModel : ViewModelBase
    {
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
    }
}
