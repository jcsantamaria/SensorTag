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
    public class KeysViewModel : ViewModelBase
    {
        protected readonly IEventAggregator _eventAggregator;

        public KeysViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            // subscriptions
            _eventAggregator.GetEvent<PubSubEvent<KeysSensor>>().Subscribe(OnKeysSensor, ThreadOption.UIThread);

        }

        private bool _powerButton;
        public bool PowerButton
        {
            get { return _powerButton; }
            set { SetProperty(ref _powerButton, value); }
        }

        private bool _optionButton;
        public bool OptionButton
        {
            get { return _optionButton; }
            set { SetProperty(ref _optionButton, value); }
        }

        private void OnKeysSensor(KeysSensor args)
        {
            PowerButton = args.PowerButton;
            OptionButton = args.OptionButton;
        }
    }
}
