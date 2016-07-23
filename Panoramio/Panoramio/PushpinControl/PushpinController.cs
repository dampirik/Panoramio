using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls.Maps;

namespace Panoramio.PushpinControl
{
    public class PushpinController
    {
        public event EventHandler PushpinSelected;

        private readonly MapControl _map;

        private List<IPushpinModel> _partners;

        public const double DistanceThreshold = 40.0;

        public PushpinController(MapControl map)
        {
            _map = map;

            //TODO временная мера. Нобходимо более четка контролировать этот процесс. 
            //Нужно мерить зум, смещение, не пересчитывать точки чаще чем раз в секунду.
            //Не пересчитывтаь при слишком маленьких величинах.
            _map.LoadingStatusChanged += OnLoadingStatusChanged;
        }

        public PushpinController(MapControl map, List<IPushpinModel> partners)
            : this(map)
        {
            UpdateChildrens(partners);
        }

        public void UpdateChildrens(List<IPushpinModel> partners)
        {
            _partners = partners;
            RenderPushpins();
        }

        public void ClearSelected()
        {
            foreach (var pushpin in _map.Children.OfType<PushpinControl>().Where(s => s.IsSelected))
            {
                pushpin.IsSelected = false;
            }
        }

        void OnLoadingStatusChanged(MapControl sender, object args)
        {
            if (sender.LoadingStatus == MapLoadingStatus.Loaded)
                RenderPushpins();
        }

        private void RenderPushpins()
        {
            if (_partners == null)
                return;

            Debug.WriteLine(">>>RenderPushpins");

            var pinsToAdd = new List<PushpinContainer>();

            // consider each pin in turn
            foreach (var pin in _partners)
            {
                //var newPinContainer = new PushpinContainer(pin,
                //  _map.LocationToViewportPoint(pin.Location));

                Windows.Foundation.Point point;
                _map.GetOffsetFromLocation(pin.Location, out point);
                var newPinContainer = new PushpinContainer(pin, point);

                if (!PointIsVisibleInMap(newPinContainer.ScreenLocation, _map))
                    continue;

                var addNewPin = true;

                // determine how close they are to existing pins
                foreach (var pinContainer in pinsToAdd)
                {
                    var distance = ComputeDistance(pinContainer.ScreenLocation, newPinContainer.ScreenLocation);

                    // if the distance threshold is exceeded, do not add this pin, instead
                    // add it to a cluster
                    if (distance < DistanceThreshold)
                    {
                        pinContainer.Merge(newPinContainer);
                        addNewPin = false;
                        break;
                    }
                }

                if (addNewPin)
                {
                    pinsToAdd.Add(newPinContainer);
                }
            }

            // asynchronously update the map
            _map.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var selectedPushpin = _map.Children.OfType<PushpinControl>().FirstOrDefault(s => s.IsSelected);

                var isCreateSelectedPushpin = false;

                _map.Children.Clear(); //TODO точки следует добавлять, удалять не нужны, а не чистить циликом.
                foreach (var projectedPin in pinsToAdd.Where(pin => PointIsVisibleInMap(pin.ScreenLocation, _map)))
                {
                    var children = projectedPin.GetMapElement();

                    var pushpin = children as PushpinControl;
                    if (pushpin != null && selectedPushpin != null)
                    {
                        if (ReferenceEquals(selectedPushpin.ParentModel, pushpin.ParentModel))
                        {
                            pushpin.IsSelected = true;
                            isCreateSelectedPushpin = true;
                        }
                    }

                    _map.Children.Add(children);

                    children.Tapped += OnChildrenTapped;
                }

                if (selectedPushpin != null && !isCreateSelectedPushpin)
                {
                    OnPushpinSelected(null);
                }
            });
        }

        private void OnChildrenTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (!(sender is BasePushpinControl))
            {
                return;
            }

            foreach (var item in _map.Children.OfType<PushpinControl>().Where(item => item.IsSelected && !ReferenceEquals(sender,item)))
            {
                item.IsSelected = false;
            }

            var cluster = sender as ClusterPushpinControl;
            if (cluster != null)
            {
                var centerLatitude = (cluster.Parents.Sum(s=>s.Location.Position.Latitude) / cluster.Parents.Count);
                var centerLongitude = (cluster.Parents.Sum(s => s.Location.Position.Longitude) / cluster.Parents.Count);
                var center = new Geopoint(new BasicGeoposition
                {
                    Longitude = centerLongitude,
                    Latitude = centerLatitude
                });

                var maxDistance = cluster.Parents.Select(parent => Helpers.Helper.CalculationByDistance(center, parent.Location)).Max();

                Debug.WriteLine("!!!ClusterTap maxDistance: {0} count: {1} zoom: {2}", maxDistance, cluster.Parents.Count, _map.ZoomLevel);
                
                OnPushpinSelected(null);

                if (maxDistance < 0.001)
                    return;//не удается раскрыть

                var nextLevel = _map.ZoomLevel + 1;
                if (maxDistance < 4)
                    nextLevel = 11;
                if (maxDistance < 2)
                    nextLevel = 12;
                if (maxDistance < 1)
                    nextLevel = 13;
                if (maxDistance < 0.5)
                    nextLevel = 14;
                if (maxDistance < 0.25)
                    nextLevel = 15;
                if (maxDistance < 0.1)
                    nextLevel = 16;
                if (maxDistance < 0.05)
                    nextLevel = 17;

                if (nextLevel > _map.MaxZoomLevel)
                    nextLevel = _map.MaxZoomLevel;

                _map.TrySetViewAsync(cluster.Location, nextLevel, 0, 0, MapAnimationKind.Bow);

                return;
            }
            
            var pushpin = sender as PushpinControl;
            if (pushpin != null)
            {
                pushpin.IsSelected = !pushpin.IsSelected;

                OnPushpinSelected(pushpin.IsSelected ? pushpin : null);
            }
        }

        private void OnPushpinSelected(PushpinControl pushpin)
        {
            var @event = PushpinSelected;
            if (@event != null)
                @event(pushpin, EventArgs.Empty);
        }

        /// <summary>
        /// Gets whether the given point is within the map bounds
        /// </summary>
        private static bool PointIsVisibleInMap(Windows.Foundation.Point point, MapControl map)
        {
            return point.X > 0 && point.X < map.ActualWidth &&
                    point.Y > 0 && point.Y < map.ActualHeight;
        }

        /// <summary>
        /// Computes the cartesian distance between points
        /// </summary>
        private double ComputeDistance(Windows.Foundation.Point p1, Windows.Foundation.Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }
    }
}
