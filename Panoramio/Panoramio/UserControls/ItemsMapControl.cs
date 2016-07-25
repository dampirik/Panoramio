using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

namespace Panoramio.UserControls
{
    public delegate void GeoBoundsChanged(GeoBounds geoBounds);

    public sealed class ItemsMapControl : Control
    {
        public const double DistanceThreshold = 40.0;

        #region Center
        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            "Center", typeof (Geopoint), typeof (ItemsMapControl), new PropertyMetadata(null, OnCenterPropertyChanged));

        private static void OnCenterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ItemsMapControl;
            if (control?._map != null && e.NewValue != e.OldValue)
            {
                control._map.Center = (Geopoint)e.NewValue;
            }
        }

        public Geopoint Center
        {
            get { return (Geopoint)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }

        }
        #endregion

        #region ZoomLevel
        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
            "ZoomLevel", typeof (double), typeof (ItemsMapControl), new PropertyMetadata(
                10, OnZoomLevelPropertyChanged));

        private static void OnZoomLevelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ItemsMapControl;
            if (control?._map != null && e.NewValue != e.OldValue)
            {
                control._map.ZoomLevel = (double) e.NewValue;
            }
        }

        public double ZoomLevel
        {
            get { return (double)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }

        }
        #endregion

        #region Items
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            "Items", typeof(ObservableCollection<IMapItem>), typeof(ItemsMapControl), new PropertyMetadata(
                null, OnItemsPropertyChanged));

        private static void OnItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ItemsMapControl;
            if (control?._map != null && e.NewValue != e.OldValue)
            {
                if (e.OldValue != null)
                    ((ObservableCollection<IMapItem>) e.OldValue).CollectionChanged -= control.OnItemsCollectionChanged;

                if (e.NewValue != null)
                    ((ObservableCollection<IMapItem>) e.NewValue).CollectionChanged += control.OnItemsCollectionChanged;
                else
                {
#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
                    control._map.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        foreach (var item in control._map.Children.OfType<MapItemControl>())
                        {
                            item.Photo = null;
                        }

                        control._map.Children.Clear();
                    });
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
                }
                control.RecreateMapItems();
            }
        }

        public ObservableCollection<IMapItem> Items
        {
            get { return (ObservableCollection<IMapItem>)GetValue(ItemsProperty); }
            set
            {
                SetValue(ItemsProperty, value);
            }

        }
        #endregion

        #region SelectedItem
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem", typeof(IMapItem), typeof(ItemsMapControl), new PropertyMetadata(null));

        public IMapItem SelectedItem
        {
            get { return (IMapItem)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }

        }
        #endregion

        public event GeoBoundsChanged GeoBoundsChanged;

        public ItemsMapControl()
        {
            DefaultStyleKey = typeof(ItemsMapControl);
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    var items = e.NewItems.OfType<IMapItem>();
                    AddMapItems(items);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null && e.OldItems.Count > 0)
                {
                    var items = e.OldItems.OfType<IMapItem>();
                    RemoveMapItems(items);
                }
            }
            else
            {
                RecreateMapItems();
            }
        }

        private MapControl _map;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _map = GetTemplateChild("Map") as MapControl;

            if (_map != null)
            {
                _map.Center = Center;
                _map.ZoomLevel = ZoomLevel;
                _map.CenterChanged += OnCenterChanged;
                _map.ZoomLevelChanged += OnZoomLevelChanged;
                _map.LoadingStatusChanged += OnLoadingStatusChanged;

                RecreateMapItems();
            }
        }
        private void OnLoadingStatusChanged(MapControl sender, object args)
        {
            if (sender.LoadingStatus == MapLoadingStatus.Loaded)
                OnCheckGeoBounds();
        }

        private void OnZoomLevelChanged(MapControl sender, object args)
        {
            ZoomLevel = _map.ZoomLevel;
        }

        private void OnCenterChanged(MapControl sender, object args)
        {
            Center = _map.Center;
        }

        private void RecreateMapItems()
        {
#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
            _map.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                                                    {
                                                                        foreach (var item in _map.Children.OfType<MapItemControl>())
                                                                        {
                                                                            item.Tapped -= OnChildrenTapped;
                                                                            item.Photo = null;
                                                                        }
                                                                        
                                                                        _map.Children.Clear();
                                                                        AddMapItems(Items);
                                                                    });
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
        }

        private void RemoveMapItems(IEnumerable<IMapItem> items)
        {
            if (items == null)
                return;

#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
            _map.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (var item in items)
                {
                    var result = _map.Children.OfType<MapItemControl>().FirstOrDefault(s => s.ParentModel.Id == item.Id);

                    if(result == null)
                        continue;

                    result.Tapped -= OnChildrenTapped;
                    result.Photo = null;

                    _map.Children.Remove(result);
                }
            });
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова

        }

        private void AddMapItems(IEnumerable<IMapItem> items)
        {
            if (items == null)
                return;
            
            var itemsToAdd = new List<MapItemControl>();
            
            foreach (var pin in items)
            {
                Point point;
                _map.GetOffsetFromLocation(pin.Location, out point);

                if (!PointIsVisibleInMap(point, _map))
                    continue;

                var newControl = new MapItemControl(pin, point);
                itemsToAdd.Add(newControl);
            }

            // asynchronously update the map
#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
            _map.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (var item in itemsToAdd.Where(pin => PointIsVisibleInMap(pin.ScreenLocation, _map)))
                {
                    _map.Children.Add(item);

                    item.Tapped += OnChildrenTapped;
                }
            });
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
        }

        private void OnChildrenTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var item = sender as MapItemControl;
            if (item != null)
            {
                if (SelectedItem != null)
                    SelectedItem = null;
                SelectedItem = item.ParentModel;
            }
        }

        private double? _oldZoom = 0;
        private Geopoint _oldCenter;

        private bool CheckChanges()
        {
            if (_oldCenter == null)
                return true;

            if (_oldZoom == null)
                return true;

            if (Math.Abs(_oldZoom.Value - _map.ZoomLevel) > 1)
                return true;

            var distanceta = Helpers.Helper.CalculationByDistance(_oldCenter, _map.Center);
            if (distanceta > 0.1)
                return true;

            return false;
        }

        private void OnCheckGeoBounds()
        {
            if (!CheckChanges())
                return;

            _oldZoom = _map.ZoomLevel;
            _oldCenter = _map.Center;

            var @event = GeoBoundsChanged;
            if (@event == null)
                return;
            var geoBounds = new GeoBounds();

            Geopoint lt;
            _map.GetLocationFromOffset(new Point(0, 0), out lt);

            Geopoint rb;
            _map.GetLocationFromOffset(new Point(_map.ActualHeight, _map.ActualWidth), out rb);
            
            geoBounds.MinX = lt.Position.Longitude;
            geoBounds.MaxX = rb.Position.Longitude;

            geoBounds.MinY = rb.Position.Latitude;
            geoBounds.MaxY = lt.Position.Latitude;

            @event(geoBounds);
        }

        /// <summary>
        /// Gets whether the given point is within the map bounds
        /// </summary>
        private static bool PointIsVisibleInMap(Point point, MapControl map)
        {
            return point.X > 0 && point.X < map.ActualWidth &&
                    point.Y > 0 && point.Y < map.ActualHeight;
        }
    }
}
