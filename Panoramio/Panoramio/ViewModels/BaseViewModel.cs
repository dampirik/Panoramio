using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Caliburn.Micro;
using Windows.UI.Xaml.Controls.Primitives;

namespace Panoramio.ViewModels
{
    public abstract class BaseViewModel : Screen
    {
        protected INavigationService NavigationService { get; private set; }

        protected BaseViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        private bool _isDataLoaded;

        public bool IsDataLoaded
        {
            get { return _isDataLoaded; }
            set { Set(ref _isDataLoaded, value); }
        }

        protected override async void OnViewReady(object view)
        {
            base.OnViewReady(view);

            await LoadData();

            IsDataLoaded = true;
        }

        protected virtual async Task LoadData()
        {
            //ignore
        }

        protected virtual bool Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;

            NotifyOfPropertyChange(propertyName);
            
            return true;
        }

        public object Result { get; set; }
        
        public void TryCloseWithResult(object result)
        {
            Result = result;

            TryClose();
        }

        public override void TryClose(bool? dialogResult = null)
        {
            GetViewCloseAction(this, Views.Values, dialogResult).OnUIThread();
        }

        private System.Action GetViewCloseAction(object viewModel, ICollection<object> views, bool? dialogResult)
        {
            var child = viewModel as IChild;
            if (child != null)
            {
                var conductor = child.Parent as IConductor;
                if (conductor != null)
                {
                    return () => conductor.CloseItem(viewModel);
                }
            }

            foreach (var contextualView in views)
            {
                if (contextualView is Popup)
                {
                    var popup = contextualView as Popup;

                    return () =>
                    {
                        popup.IsOpen = false;
                    };
                }

                var viewType = contextualView.GetType();

                var closeMethod = viewType.GetRuntimeMethod("Close", new Type[0]);

                if (closeMethod != null)
                    return () =>
                    {
                        closeMethod.Invoke(contextualView, null);
                    };

                var isOpenProperty = viewType.GetRuntimeProperty("IsOpen");

                if (isOpenProperty != null)
                {
                    return () => isOpenProperty.SetValue(contextualView, false, null);
                }
            }

            return () => LogManager.GetLog(typeof(Screen)).Info("TryClose requires a parent IConductor or a view with a Close method or IsOpen Property.");
        }
    }
}
