using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

namespace Panoramio.UserControls
{
    public class MapItemControl : Control
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            "Location", typeof(Geopoint), typeof(MapItemControl),
            new PropertyMetadata(default(Geopoint), LocationPropertyCallback));

        private static void LocationPropertyCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            MapControl.SetLocation(dependencyObject, (Geopoint)dependencyPropertyChangedEventArgs.NewValue);
        }

        public Geopoint Location
        {
            get { return (Geopoint)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public static readonly DependencyProperty AnchorProperty = DependencyProperty.Register(
            "Anchor", typeof(Point), typeof(MapItemControl),
            new PropertyMetadata(default(Point), AnchorPropertyCallback));

        private static void AnchorPropertyCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            MapControl.SetNormalizedAnchorPoint(dependencyObject, (Point)dependencyPropertyChangedEventArgs.NewValue);
        }

        public Point Anchor
        {
            get { return (Point)GetValue(AnchorProperty); }
            set { SetValue(AnchorProperty, value); }
        }
        
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(int), typeof(MapItemControl), new PropertyMetadata(false));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public IMapItem ParentModel { get; private set; }

        public Point ScreenLocation { get; private set; }

        public MapItemControl(IMapItem parent, Point location)
        {
            DefaultStyleKey = typeof(MapItemControl);
            Anchor = new Point(0.5, 1);
            DataContext = this;

            Location = parent.Location;
            ScreenLocation = location;
            ParentModel = parent;
        }
    }
}
