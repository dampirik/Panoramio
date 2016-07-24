using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Caliburn.Micro;
using Panoramio.Common;
using Panoramio.ViewModels;
using Panoramio.Views;

namespace Panoramio
{
    /// <summary>
    /// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
    /// </summary>
    sealed partial class App
    {
        /// <summary>
        /// Инициализирует одноэлементный объект приложения.  Это первая выполняемая строка разрабатываемого
        /// кода; поэтому она является логическим эквивалентом main() или WinMain().
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);

            InitializeComponent();
        }

        private WinRTContainer _container;

        protected override void Configure()
        {
           // MessageBinder.SpecialValues.Add("$clickeditem", c => ((ItemClickEventArgs)c.EventArgs).ClickedItem);

            _container = new WinRTContainer();

            _container.RegisterWinRTServices();

            _container.PerRequest<MainViewModel>();
            _container.PerRequest<WindowManager>();
        }

        protected override object GetInstance(Type service, string key)
        {
            var instance = _container.GetInstance(service, key);
            if (instance != null)
                return instance;
            throw new Exception("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void PrepareViewFirst(Frame rootFrame)
        {
            _container.RegisterNavigationService(rootFrame);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
                return;

            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            DisplayRootView<MainView>();
        }

        /// <summary>
        /// Вызывается при обычном запуске приложения пользователем.  Будут использоваться другие точки входа,
        /// например, если приложение запускается для открытия конкретного файла.
        /// </summary>
        /// <param name="e">Сведения о запросе и обработке запуска.</param>
//        protected override void OnLaunched(LaunchActivatedEventArgs args)
//        {
//            Initialize();

//#if DEBUG
//            if (System.Diagnostics.Debugger.IsAttached)
//            {
//                DebugSettings.EnableFrameRateCounter = true;
//            }
//#endif
//            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
//            {
//                return;
//            }

//            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

//            //Frame rootFrame = Window.Current.Content as Frame;

//            //// Не повторяйте инициализацию приложения, если в окне уже имеется содержимое,
//            //// только обеспечьте активность окна
//            //if (RootFrame == null)
//            //{
//            //    // Создание фрейма, который станет контекстом навигации, и переход к первой странице
//            //    RootFrame = new Frame();

//            //    RootFrame.NavigationFailed += OnNavigationFailed;

//            //    if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
//            //    {
//            //        //TODO: Загрузить состояние из ранее приостановленного приложения
//            //    }

//            //    // Размещение фрейма в текущем окне
//            //    Window.Current.Content = rootFrame;
//            //}

//            if (RootFrame.Content == null)
//            {
//                DisplayRootView<MainView>();
//            }
//        }
        
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
        
        protected override void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            var ex = e.Exception;

            var error = ex.Message + "\n" + ex.StackTrace;

            base.OnUnhandledException(sender, e);

#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                      {
#if DEBUG
                          await new MessageDialog(error, "Error").ShowAsync();
#else
                    await  new MessageDialog("We are having some temporary issues. Please try again later. Thanks!", "Error").ShowAsync();
#endif

                      });
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
        }
    }
}
