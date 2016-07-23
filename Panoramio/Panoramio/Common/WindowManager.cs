using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Caliburn.Micro;
using Panoramio.ViewModels;

namespace Panoramio.Common
{
    /// <summary>
    /// WindowManager class.
    /// </summary>
    public class WindowManager
    {
        public virtual Task<T2> ShowDialog<T1, T2>(CancellationToken cancellationToken, IDictionary<string, object> settings = null)
        {
            var viewModel = IoC.Get<T1>();

            return ShowDialog<T2>(viewModel, cancellationToken, null, settings);
        }

        public virtual Task ShowDialog<T>(CancellationToken cancellationToken, IDictionary<string, object> settings = null)
        {
            var viewModel = IoC.Get<T>();

            return ShowDialog<object>(viewModel, cancellationToken, null, settings);
        }

        private static readonly List<Popup> Popups = new List<Popup>();

        public virtual Task<T> ShowDialog<T>(object rootModel, CancellationToken cancellationToken, object context = null,
            IDictionary<string, object> settings = null)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();

            var popup = CreatePopup(rootModel, settings);

            cancellationToken.Register(() =>
            {
                if (popup.IsOpen)
                {
                    popup.IsOpen = false;
                }
            });
            
            var view = ViewLocator.LocateForModel(rootModel, popup, context);

            var grid = new Grid();

            grid.Children.Add(view);
            popup.Child = grid;

            popup.SetValue(View.IsGeneratedProperty, true);

            ViewModelBinder.Bind(rootModel, popup, null);

            var activatable = rootModel as IActivate;
            if (activatable != null)
            {
                activatable.Activate();
            }

            var deactivator = rootModel as IDeactivate;
            if (deactivator != null)
            {
                var navigationService = IoC.Get<INavigationService>();
                
                EventHandler<Windows.UI.Core.BackRequestedEventArgs> onBackRequested = (o, args) =>
                {
                    if (args.Handled)
                        return;

                    var item = Popups[Popups.Count - 1];
                    if (item.IsOpen)
                    {
                        item.IsOpen = false;
                        args.Handled = true;
                    }
                };

                navigationService.BackRequested += onBackRequested;

                EventHandler<object> onClosed = null;
                onClosed = (s, e) =>
                {
                    popup.Closed -= onClosed;
                    navigationService.BackRequested -= onBackRequested;

                    Popups.Remove(popup);

                    var model = rootModel as BaseViewModel;
                    if (model != null && model.Result != null)
                    {
                        taskCompletionSource.TrySetResult((T)model.Result);
                    }
                    else
                    {
                        taskCompletionSource.TrySetResult(default(T));
                    }

                    deactivator.Deactivate(true);
                };

                popup.Closed += onClosed;
            }

            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            
            var child = popup.Child as Grid;

            if (child != null)
            {
                child.Width = bounds.Width;
                child.Height = bounds.Height;
            }

            //var statusbar = "Windows.UI.ViewManagement.StatusBar";
            //if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(statusbar))
            //{
            //    Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ShowAsync();
            //}

            //TODO
            //var statusBar = StatusBar.GetForCurrentView();
            //var occludedRect = statusBar.OccludedRect;
            //if (occludedRect.Height > 0)
            //{
            //    popup.VerticalOffset = occludedRect.Height;
            //}

            popup.IsOpen = true;

            Popups.Add(popup);

            return taskCompletionSource.Task;
        }

        protected virtual Popup CreatePopup(object rootModel, IDictionary<string, object> settings)
        {
            var popup = new Popup {IsLightDismissEnabled = false};

            ApplySettings(rootModel, settings);

            return popup;
        }

        private bool ApplySettings(object target, IEnumerable<KeyValuePair<string, object>> settings)
        {
            if (settings != null)
            {
                var type = target.GetType();

                foreach (var pair in settings)
                {
                    var propertyInfo = type.GetPropertyCaseInsensitive(pair.Key);

                    if (propertyInfo != null)
                    {
                        propertyInfo.SetValue(target, pair.Value, null);
                    }
                }

                return true;
            }

            return false;
        }
    }
}
