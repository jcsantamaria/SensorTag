using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Microsoft.Practices.Unity;

using Prism.Unity.Windows;

namespace SensorTagPi
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : PrismUnityApplication
    {
        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            // Navigate to the initial page
            NavigationService.Navigate("Devices", null);

            Window.Current.Activate();

            // connect by default
            //var mainvm = Container.Resolve<ViewModels.MainPageViewModel>();
            //mainvm.DoConnectCommand();

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        protected override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            // register services
            var logger = new Log.Logger();
            var sensorService = new Models.SensorTagService(logger, EventAggregator);
            var iotService    = new Models.IoTService(logger, EventAggregator);

            Container.RegisterInstance<Core.Interfaces.ILogger>(logger);
            Container.RegisterInstance<Models.ISensorTagService>(sensorService);
            Container.RegisterInstance<Models.IIoTService>(iotService);

            // JCS: no need to define these: default implementation already does the job as long as views and viewmodels follow name conventions
            // define ViewModelLocator
            //ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            //{
            //    var viewModelTypeName = string.Format(CultureInfo.InvariantCulture, "RasPiApp.ViewModels.{0}ViewModel, RasPiApp, Version=1.0.0.0, Culture=neutral", viewType.Name);
            //    var viewModelType = Type.GetType(viewModelTypeName);

            //    return viewModelType;
            //});
            //ViewModelLocationProvider.SetDefaultViewModelFactory((viewModelType) => {
            //    return Container.Resolve(viewModelType);
            //});

            return base.OnInitializeAsync(args);
        }
    }
}
