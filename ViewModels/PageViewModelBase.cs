using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Events;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using Prism.Commands;

using SensorTagPi.Core.Interfaces;

namespace SensorTagPi.ViewModels
{
    public class PageViewModelBase : ViewModelBase
    {
        protected readonly ILogger            _logger;
        protected readonly IEventAggregator   _eventAggregator;
        protected readonly INavigationService _navigation;
        protected readonly TaskScheduler      _uiContext;

        public PageViewModelBase(ILogger logger, IEventAggregator eventAggregator, INavigationService navigation)
        {
            _logger          = logger;
            _eventAggregator = eventAggregator;
            _navigation      = navigation;
            _uiContext       = TaskScheduler.FromCurrentSynchronizationContext();

            // command implementation
            NavigateCommand = new DelegateCommand<string>(DoNavigateCommand);
        }

        #region Commands
        public DelegateCommand<string> NavigateCommand { get; private set; }
        #endregion

        #region Command Implemenation
        void DoNavigateCommand(string pageToken)
        {
            _navigation.Navigate(pageToken, null);
        }
        #endregion

    }
}
